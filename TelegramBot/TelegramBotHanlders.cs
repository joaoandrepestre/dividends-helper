using DividendsHelper.States;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.TelegramBot;
public class TelegramBotHanlders {
    private static readonly string _tokenName = "DH_TOKEN";
    private static readonly ReceiverOptions _receiverOptions = new() {
        AllowedUpdates = Array.Empty<UpdateType>(),
    };

    private string _accessToken => Environment.GetEnvironmentVariable(_tokenName, Process) ??
        Environment.GetEnvironmentVariable(_tokenName, Machine) ?? "";
    private readonly CancellationTokenSource _cancel = new();

    private TelegramBotClient _botClient;

    private State _state;

    public TelegramBotHanlders(State state) {
        _state = state;
    }

    public async Task Load() {
        Console.WriteLine("Loading Telegram Bot...");
        if (string.IsNullOrEmpty(_accessToken)) {
            Console.WriteLine($"ERROR - Could not retrieve acess token. Remember to set the enviroment variable {_tokenName}.");
            return;
        }
        _botClient = new TelegramBotClient(_accessToken);
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: _receiverOptions,
            cancellationToken: _cancel.Token
        );

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}...");
        Console.WriteLine("Loading Telegram Bot done.");
    }

    public void Stop() {
        _cancel.Cancel();
    }

    private async Task HandleMonitor(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
        var messageText = message.Text;
        var args = messageText?.Split(' ');
        if (args?.Length < 2) { // not enough arguments
            Console.WriteLine($"Received /monitor command without args from {message.From?.Username ?? ""}");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"The /monitor command expects at least one argument\n/monitor symbol\n\nTry:\n/monitor PETR4",
                cancellationToken: cancellationToken);
            return;
        }
        var symbol = args[1];
        var exists = await _state.Monitor(symbol);
        if (!exists) {
            Console.WriteLine($"Received /monitor command for non existent symbol {symbol} from {message.From?.Username ?? ""}");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Could not find {symbol} at B3\nPlease check the spelling",
                cancellationToken: cancellationToken);
            return;
        }
        var summary = _state.CashProvisions.GetSummary(symbol.ToUpper(), DateTime.Today.AddYears(-1));
        await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                parseMode: ParseMode.MarkdownV2,
                text: $"Started monitoring {symbol.ToUpper()}\nHere's the most recent data:\n\n``` {summary} ```",
                cancellationToken: cancellationToken);
    }


    private async Task HandleSummary(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
        var messageText = message.Text;
        var args = messageText?.Split(' ');
        if (args?.Length < 2) { // not enough arguments
            Console.WriteLine($"Received /summary command without args from {message.From?.Username ?? ""}");
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"The /summary command expects at least one argument\n/summary symbol\n/summary symbol minDate\n/summary symbol minDate maxDate\n\nTry:\n/summary PETR4",
                cancellationToken: cancellationToken);
            return;
        }
        var symbol = args[1];
        if (!_state.MonitoredSymbols.Contains(symbol)) {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{symbol} is not yet monitored. \n\nTry:\n/monitor {symbol}",
                cancellationToken: cancellationToken);
            return;
        }

        DateTime minDate = new DateTime(1, 1, 1);
        DateTime maxDate = DateTime.Today;
        if (args?.Length >= 3) // minDate
            if (!DateTime.TryParse(args[2], out minDate))
                minDate = new DateTime(1, 1, 1);
        if (args?.Length >= 4) // maxDate
            if (!DateTime.TryParse(args[3], out maxDate))
                maxDate = DateTime.Today;

        var summary = _state.CashProvisions.GetSummary(symbol, minDate, maxDate);
        await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                parseMode: ParseMode.MarkdownV2,
                text: $"``` {summary} ```",
                cancellationToken: cancellationToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;
        var commandEntity = message.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);
        if (commandEntity == null)
            return;

        var chatId = message.Chat.Id;
        var user = message.From?.Username ?? "";
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {user}.");

        // Find which command was passed

        var command = message.EntityValues?.ToArray()[commandEntity.Offset];

        // Route commands
        if (command == "/summary") {
            await HandleSummary(botClient, message, cancellationToken);
            return;
        }
        if (command == "/monitor") {
            await HandleMonitor(botClient, message, cancellationToken);
            return;
        }
        // recommend portifolio command
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
        var ErrorMessage = exception switch {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
