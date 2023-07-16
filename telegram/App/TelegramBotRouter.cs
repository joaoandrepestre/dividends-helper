using DividendsHelper.Telegram.ApiClient;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DividendsHelper.Telegram.Handlers;
using DividendsHelper.Telegram.Messages;
using DividendsHelper.Telegram.Utils;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.Telegram;
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

    public TelegramBotRouter(DhApiClient api) {
        RegisterCommandHandlers(api);
    }

    private void RegisterCommandHandlers(DhApiClient api) {
        var handlers = typeof(Program).Assembly.GetTypesWithAttribute<TelegramMessageHandlerAttribute>();
        foreach (var handlerType in handlers)
        {
            var command = handlerType.GetAttribute<TelegramMessageHandlerAttribute>()?.Command;
            var handler = Activator.CreateInstance(handlerType, api) as IBaseHandler;
            if (command is not null && handler is not null) {
                Console.WriteLine($"Registering handler for command {command}: {handlerType.Name}");
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
        Console.WriteLine("Loading Telegram Bot...");
        if (string.IsNullOrEmpty(AccessToken)) {
            Console.WriteLine($"ERROR - Could not retrieve access token. Remember to set the environment variable {TokenName}.");
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
        Console.WriteLine($"Start listening for @{me.Username}...");
        Console.WriteLine("Loading Telegram Bot done.");
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
            Console.WriteLine($"Received an unrecognized command '{command}' in chat {message.Chat.Id} from {message.From?.Username ?? ""}.");
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

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
