using Beef.Types.Requests;
using Crudite.Types;

namespace DividendsHelper.Models.Core;

public class TradingData : IBaseModel<SymbolDate>
{
    public SymbolDate Id => new(Symbol, ReferenceDate);
    public string Symbol { get; set; } = "";
    public DateTime ReferenceDate { get; set; }
    public decimal ClosingPrice { get; set; }
}