namespace DividendsHelper.Models;
public class MonitorCommand : BaseTelegramMessage {
    public string Symbol { get; set; }
    public override string CommandName() => "/monitor";
}
