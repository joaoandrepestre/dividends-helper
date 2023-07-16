namespace DividendsHelper.Telegram.Messages;
public class MonitorCommand : BaseTelegramMessage
{
    [TelegramMessageArgument(1, true, "PETR4")]
    public string Symbol { get; set; } = "";
    public override string CommandName() => "/monitor";
}
