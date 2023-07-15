namespace DividendsHelper.Core.Models; 

public class PortfolioCommand : BaseTelegramMessage {
    [TelegramMessageArgument(1)]
    public decimal Limit { get; set; } = 20;
    //public IEnumerable<string>? Symbols;
    [TelegramMessageArgument(2)]
    public decimal Investment { get; set; } = 10000;
    [TelegramMessageArgument(3)]
    public DateTime MinDate { get; set; } = DateTime.MinValue;
    [TelegramMessageArgument(4)]
    public DateTime MaxDate { get; set; } = DateTime.Today;
    public override string CommandName() => "/portfolio";
}