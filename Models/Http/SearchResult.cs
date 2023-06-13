namespace DividendsHelper.Models;
public class SearchResult {
    public int CodeCvm { get; set; }
    public string IssuingCompany { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string TradingName { get; set; } = "";
    public string Cnpj { get; set; } = "";
    public int MarketIndicator { get; set; }
    public string TypeBDR { get; set; } = "";
    public string DateListing { get; set; } = "";
    public string Status { get; set; } = "";
    public string Segment { get; set; } = "";
    public string SegmentEng { get; set; } = "";
    public int Type { get; set; }
    public string Market { get; set; } = "";
}
