namespace DividendsHelper.Models;

public class TradingDataResult {
    public string TickerSymbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public int TradeId { get; set; }
    public DateTime EntryDate { get; set; }
    public TimeSpan EntryTime { get; set; }
    public DateTime TradingDateTime => EntryDate.Add(EntryTime);
    public string EntryBuyer { get; set; }
    public string EntrySeller { get; set; }
}