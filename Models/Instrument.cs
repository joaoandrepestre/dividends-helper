namespace DividendsHelper.Models;
public class Instrument : IBaseModel<string> {
    public string Id => Symbol;

    public string Symbol { get; set; }
    public string TradingName { get; set; }

}
