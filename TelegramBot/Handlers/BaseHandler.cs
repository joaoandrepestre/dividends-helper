using DividendsHelper.Models;
using DividendsHelper.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DividendsHelper.TelegramBot.Handlers;
public abstract class BaseHandler<T> where T : BaseTelegramMessage, new() {
    protected State _state;

    public BaseHandler(State state) {
        _state = state;
    }
    protected virtual T Parse(Message message) {
        return new T {
            ChatId = message.Chat.Id,
            Sender = message.From?.Username ?? "",
            Text = message.Text ?? "",
            Valid = true,
        };
    }

    protected abstract Task<string> GetResponse(T command);

    public async Task Handle(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
        var command = Parse(message);
        Console.WriteLine($"Received a '{command.Text}' message in chat {command.ChatId} from {command.Sender}.");
        if (!command.Valid) {
            await botClient.SendTextMessageAsync(
                chatId: command.ChatId,
                text: command.Error,
                cancellationToken: cancellationToken);
            return;
        }

        var res = await GetResponse(command);
        await botClient.SendTextMessageAsync(
                chatId: command.ChatId,
                parseMode: ParseMode.MarkdownV2,
                text: res,
                cancellationToken: cancellationToken);
    }
}

