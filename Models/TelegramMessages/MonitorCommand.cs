namespace DividendsHelper.Models;
public class MonitorCommand : BaseTelegramMessage {
    [TelegramMessageArgument(1, true, "PETR4")]
    public string Symbol { get; set; }
    public override string CommandName() => "/monitor";
}
