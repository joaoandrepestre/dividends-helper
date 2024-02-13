using Crudite.Types;

namespace DividendsHelper.Models.Core;
public class Instrument : IBaseModel<string> {
    public string Id => Symbol;

    public string Symbol { get; init; } = "";
    public string TradingName { get; init; } = "";

}
