using DividendsHelper.Models;
using DividendsHelper.States;
using DividendsHelper.TelegramBot.Handlers;
using DividendsHelper.Utils;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.TelegramBot;
public class TelegramBotRouter {
    private const string TokenName = "DH_TOKEN";

    private static readonly ReceiverOptions ReceiverOptions = new() {
        AllowedUpdates = new [] { UpdateType.Message },
    };

    private static string AccessToken => Environment.GetEnvironmentVariable(TokenName, EnvironmentVariableTarget.Process) ??
        Environment.GetEnvironmentVariable(TokenName, Machine) ?? "";
    private readonly CancellationTokenSource _cancel = new();

    private TelegramBotClient? _botClient;
    private readonly Dictionary<string, IBaseHandler> _commandHandlers = new();

    public TelegramBotRouter(State state) {
        RegisterCommandHandlers(state);
    }

    private void RegisterCommandHandlers(State state)
    {
        var handlers = typeof(Program).Assembly.GetTypesWithAttribute<TelegramMessageHandlerAttribute>();
        foreach (var handlerType in handlers)
        {
            var command = handlerType.GetAttribute<TelegramMessageHandlerAttribute>()?.Command;
            var handler = Activator.CreateInstance(handlerType, state) as IBaseHandler;
            if (command is not null && handler is not null) {
                Logger.Log($"Registering handler for command {command}: {handlerType.Name}");
                _commandHandlers.Add(command, handler);
            }
        }   
    }

    private async Task RegisterCommandsWithBot() {
        if (_botClient is null) return;
        var commands = _commandHandlers.Select(kvp => {
            var description = kvp.Value
                .GetType()
                .GetAttribute<TelegramMessageHandlerAttribute>()?
                .Description ?? "";
            return new BotCommand
            {
                Command = kvp.Key,
                Description = description,
            };
        });
        await _botClient.SetMyCommandsAsync(commands);
    }

    public async Task<bool> Load() {
        Logger.Log("Loading Telegram Bot...");
        if (string.IsNullOrEmpty(AccessToken)) {
            Logger.Log($"ERROR - Could not retrieve access token. Remember to set the environment variable {TokenName}.", LogLevel.Error);
            return false;
        }

        _botClient = new TelegramBotClient(AccessToken);
        await RegisterCommandsWithBot();
        _botClient.StartReceiving(
            updateHandler: RouteMessages,
            pollingErrorHandler: HandleErrors,
            receiverOptions: ReceiverOptions,
            cancellationToken: _cancel.Token
        );

        var me = await _botClient.GetMeAsync();
        Logger.Log($"Start listening for @{me.Username}...");
        Logger.Log("Loading Telegram Bot done.");
        return true;
    }

    public void Stop() {
        _cancel.Cancel();
    }

    private async Task RouteMessages(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        var args = messageText.Split(" ");
        if (args.Length == 0)
            return;

        var command = args[0];
        if (!_commandHandlers.TryGetValue(command, out var handler))
        {
            Logger.Log($"Received an unrecognized command '{command}' in chat {message.Chat.Id} from {message.From?.Username ?? ""}.");
            return;
        }
        
        await handler.Handle(botClient, message, cancellationToken);
    }

    private static Task HandleErrors(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
        var errorMessage = exception switch {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Logger.Log(errorMessage, LogLevel.Error);
        return Task.CompletedTask;
    }
}
