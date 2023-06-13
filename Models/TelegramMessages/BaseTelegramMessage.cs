using Telegram.Bot.Types;

namespace DividendsHelper.Models;

public abstract class BaseTelegramMessage {
    public ChatId ChatId { get; set; }

    public string Sender { get; set; }

    public string Text { get; set; }

    public bool Valid { get; set; }

    private string _error = "";
    public string Error {
        get => Valid ? "" : $"{CommandName()} {_error}\n\n{Suggestion()}";
        set => _error = value;
    }

    public abstract string CommandName();

    public abstract string Suggestion();
}
