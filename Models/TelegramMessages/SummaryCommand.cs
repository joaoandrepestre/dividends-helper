namespace DividendsHelper.Models;
public class SummaryCommand : BaseTelegramMessage {
    public string Symbol { get; set; }
    public DateTime MinDate { get; set; } = DateTime.MinValue;
    public DateTime MaxDate { get; set; } = DateTime.Today;
    public override string CommandName() => "/summary";
}
