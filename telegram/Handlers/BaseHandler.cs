using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using DividendsHelper.Telegram.Messages;
using DividendsHelper.Telegram.ApiClient;
using DividendsHelper.Telegram.Utils;
using Telegram.Bot.Types;

namespace DividendsHelper.Telegram.Handlers;

public interface IBaseHandler {
    Task Handle(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}
public abstract class BaseHandler<T> : IBaseHandler 
    where T : BaseTelegramMessage, new() {
    protected readonly DhApiClient Api;

    protected BaseHandler(DhApiClient api) {
        Api = api;
    }

    protected virtual T ValidateArgs(T command) => command;
    private T Parse(Message message) {
        var expectedArgs = typeof(T)
            .GetProperties()
            .Where(p => p.HasAttribute<TelegramMessageArgumentAttribute>())
            .OrderBy(p => {
                var att = p.GetAttribute<TelegramMessageArgumentAttribute>();
                return att?.Position ?? 0;
            });
        
        var ret = new T {
            ChatId = message.Chat.Id,
            Sender = message.From?.Username ?? "",
            Text = message.Text ?? "",
            Valid = true,
        };
        var args = ret.Text.Split(" ");
        var emptyRequired = new List<PropertyInfo>();
        foreach (var prop in expectedArgs) {
            var att = prop.GetAttribute<TelegramMessageArgumentAttribute>();
            var i = att?.Position;
            if (i is null) continue;
            string? arg = null;
            try {
                arg = args[(int)i];
            } catch (IndexOutOfRangeException) {
                Console.WriteLine($"No value passed for argument {prop.Name}");
            }
            if (arg is null || !arg.TryParse(out var v, prop.PropertyType)) {
                Console.WriteLine($"Invalid value for type {prop.PropertyType}: {arg ?? "null"}");
                if (att?.Required ?? false) {
                    emptyRequired.Add(prop);
                }
                continue;
            }
            prop.SetValue(ret, v);
        }

        if (emptyRequired.Any()) {
            ret.Valid = false;
            var names = string.Join(", ", emptyRequired.Select(i => i.Name));
            var examples = string.Join(" ", emptyRequired.Select(i => i.GetAttribute<TelegramMessageArgumentAttribute>()?.ExambleIfEmpty ?? ""));
            ret.Error = $"expects {names}\n\nTry `{ret.CommandName()} {examples}`";
        }
        return ValidateArgs(ret);
    }

    protected abstract Task<string> GetResponse(T command);

    public async Task Handle(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
        var command = Parse(message);
        Console.WriteLine($"Received a '{command.Text}' message in chat {command.ChatId} from {command.Sender}.");
        if (!command.Valid) {
            await botClient.SendTextMessageAsync(
                chatId: command.ChatId,
                parseMode: ParseMode.MarkdownV2,
                disableWebPagePreview: true,
                text: command.Error,
                cancellationToken: cancellationToken);
            return;
        }

        var res = await GetResponse(command);
        await botClient.SendTextMessageAsync(
                chatId: command.ChatId,
                parseMode: ParseMode.MarkdownV2,
                disableWebPagePreview: true,
                text: res,
                cancellationToken: cancellationToken);
    }
}

