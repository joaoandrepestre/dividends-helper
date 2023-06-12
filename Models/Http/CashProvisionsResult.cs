namespace DividendsHelper.Models;
public class CashProvisionsResult {
    public string TypeStock { get; set; }
    public string DateApproval { get; set; }
    public decimal? ValueCash { get; set; }
    public decimal? Ratio { get; set; }
    public string CorporateAction { get; set; }
    public string LastDatePriorEx { get; set; }
    public decimal? ClosingPricePriorEx { get; set; }
    public decimal? QuotedPerShares { get; set; }
    public decimal? CorporateActionPrice { get; set; }
    public DateTime LastDateTimePriorEx { get; set; }
}

