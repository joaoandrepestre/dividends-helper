using Telegram.Bot.Types;

namespace DividendsHelper.Core.Models;

public abstract class BaseTelegramMessage
{
    public ChatId ChatId { get; init; } = default!;

    public string Sender { get; init; } = "";

    public string Text { get; init; } = "";

    public bool Valid { get; set; }

    private string _error = "";
    public string Error {
        get => Valid ? "" : $"{CommandName()} {_error}";
        set => _error = value;
    }

    public abstract string CommandName();
}
