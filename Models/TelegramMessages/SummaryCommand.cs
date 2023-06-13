namespace DividendsHelper.Models;
public class SummaryCommand : BaseTelegramMessage
{
    [TelegramMessageArgument(1, true, "PETR4")]
    public string Symbol { get; set; } = "";
    [TelegramMessageArgument(2)]
    public DateTime MinDate { get; set; } = DateTime.MinValue;
    [TelegramMessageArgument(3)]
    public DateTime MaxDate { get; set; } = DateTime.Today;

    public override string CommandName() => "/summary";
}
