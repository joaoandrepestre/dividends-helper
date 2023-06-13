namespace DividendsHelper.Models;
public class Instrument : IBaseModel<string> {
    public string Id => Symbol;

    public string Symbol { get; init; } = "";
    public string TradingName { get; init; } = "";

}
