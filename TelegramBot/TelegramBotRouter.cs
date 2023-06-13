using DividendsHelper.States;
using DividendsHelper.TelegramBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.TelegramBot;
public class TelegramBotRouter {
    private static readonly string _tokenName = "DH_TOKEN";
    private static readonly ReceiverOptions _receiverOptions = new() {
        AllowedUpdates = Array.Empty<UpdateType>(),
    };

    private static string AccessToken => Environment.GetEnvironmentVariable(_tokenName, Process) ??
        Environment.GetEnvironmentVariable(_tokenName, Machine) ?? "";
    private readonly CancellationTokenSource _cancel = new();

    private TelegramBotClient _botClient;

    private readonly State _state;
    private readonly MonitorCommandHandler _monitorCommandHandler;
    private readonly SummaryCommandHandler _summaryCommandHandler;

    public TelegramBotRouter(State state) {
        _state = state;
        _monitorCommandHandler = new MonitorCommandHandler(_state);
        _summaryCommandHandler = new SummaryCommandHandler(_state);
    }

    public async Task<bool> Load() {
        Logger.Log("Loading Telegram Bot...");
        if (string.IsNullOrEmpty(AccessToken)) {
            Logger.Log($"ERROR - Could not retrieve acess token. Remember to set the enviroment variable {_tokenName}.", LogLevel.Error);
            return false;
        }
        _botClient = new TelegramBotClient(AccessToken);
        _botClient.StartReceiving(
            updateHandler: RouteMessages,
            pollingErrorHandler: HandleErrors,
            receiverOptions: _receiverOptions,
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
        if (args is null || args.Length == 0)
            return;

        var command = args[0];
        if (command == "/monitor") {
            await _monitorCommandHandler.Handle(botClient, message, cancellationToken);
            return;
        }
        if (command == "/summary") {
            await _summaryCommandHandler.Handle(botClient, message, cancellationToken);
            return;
        }
        // recommend portifolio command

        Logger.Log($"Received an unrecognized command '{command}' in chat {message.Chat.Id} from {message.From?.Username ?? ""}.");
    }

    private Task HandleErrors(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
        var ErrorMessage = exception switch {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Logger.Log(ErrorMessage, LogLevel.Error);
        return Task.CompletedTask;
    }
}
