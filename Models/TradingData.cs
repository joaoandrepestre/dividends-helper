namespace DividendsHelper.Models;

public readonly record struct SymbolDate(string Symbol, DateTime ReferenceDate) {
    public override string ToString() {
        return $"{Symbol}|{ReferenceDate}";
    }
};
public class TradingData : IBaseModel<SymbolDate>
{
    public SymbolDate Id => new(Symbol, ReferenceDate);
    public string Symbol { get; set; } = "";
    public DateTime ReferenceDate { get; set; }
    public decimal ClosingPrice { get; set; }
}