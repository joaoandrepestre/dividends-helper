namespace DividendsHelper.Models;

public class SimulationCommand : BaseTelegramMessage {
    [TelegramMessageArgument(1, true, "PETR4")]
    public string Symbol { get; set; } = "";

    [TelegramMessageArgument(2)] 
    public decimal Investment { get; set; } = 1000;
    [TelegramMessageArgument(3)]
    public DateTime MinDate { get; set; } = DateTime.MinValue;
    [TelegramMessageArgument(4)]
    public DateTime MaxDate { get; set; } = DateTime.Today;
    public override string CommandName() => "/simulate";
}