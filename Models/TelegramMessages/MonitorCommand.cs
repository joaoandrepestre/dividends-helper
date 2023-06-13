namespace DividendsHelper.Models;
public class MonitorCommand : BaseTelegramMessage {
    public string Symbol { get; set; }
    public override string CommandName() => "/monitor";
    public override string Suggestion() => $"Try {CommandName()} PETR4";
}
