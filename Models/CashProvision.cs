using DividendsHelper.Utils;

namespace DividendsHelper.Models;

public class CashProvision : IBaseModel<Guid> {
    public Guid Id { get; set; }
    public string Symbol { get; set; }
    public DateTime ReferenceDate { get; set; }
    public decimal ValueCash { get; set; }
    public decimal CorporateActionPrice { get; set; }
    public string CorporateAction { get; set; }

    public CashProvision() {
        Id = Guid.NewGuid();
    }

    public override string ToString() => $"{Symbol} | {ReferenceDate.DateString()}: Value = {ValueCash:0.00} R$/unt, Pct of Mkt Price = {CorporateActionPrice:0.000}%";

}

// TODO add std deviation and etc.
public class CashProvisionSummary {
    public string Symbol { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int Days => FirstCashProvisionDate.DaysUntill(LastCashProvisionDate);
    public int Months => FirstCashProvisionDate.MonthsUntill(LastCashProvisionDate);
    public int Years => FirstCashProvisionDate.YearsUntill(LastCashProvisionDate);

    public int TotalCashProvisionCount { get; set; }

    public decimal TotalValueCash { get; set; }
    public decimal AverageValueCash => SafeDivision(TotalValueCash, TotalCashProvisionCount);
    public decimal DailyAverageValueCash => SafeDivision(TotalValueCash, Days);
    public decimal MonthlyAverageValueCash => SafeDivision(TotalValueCash, Months);
    public decimal YearlyAverageValueCash => SafeDivision(TotalValueCash, Years);

    public decimal TotalCorporateActionPrice { get; set; }
    public decimal AverageCorporateActionPrice => SafeDivision(TotalCorporateActionPrice, TotalCashProvisionCount);
    public decimal DailyAverageCorporateActionPrice => SafeDivision(TotalValueCash, Days);
    public decimal MonthlyAverageCorporateActionPrice => SafeDivision(TotalCorporateActionPrice, Months);
    public decimal YearlyAverageCorporateActionPrice => SafeDivision(TotalCorporateActionPrice, Years);


    public CashProvision? FirstCashProvision { get; set; }
    public DateTime FirstCashProvisionDate => FirstCashProvision?.ReferenceDate ?? default;
    public CashProvision? LastCashProvision { get; set; }
    public DateTime LastCashProvisionDate => LastCashProvision?.ReferenceDate ?? default;
    public decimal DailyAverageCashProvisionPeriod => SafeDivision(Days, TotalCashProvisionCount - 1);
    public decimal MonthlyAverageCashProvisionPeriod => SafeDivision(Months, TotalCashProvisionCount - 1);
    public decimal YearlyAverageCashProvisionPeriod => SafeDivision(Years, TotalCashProvisionCount - 1);

    private static string _template = @"Summary for {0} from {1} to {2}:
    Last: {17}
    First: {18}
    
    Days = {3}, Months = {4}, Years = {5}
    Period: {14:0.00} days // {15:0.00} months // {16:0.00} years

    Cash provisions payed = {6}, Total value payed = {7:0.00} R$/unt

    Daily Avg: Value = {8:0.00} R$/unt, Pct of Mkt Price = {9:0.000}%
    Monthly Avg: Value = {10:0.00}, Pct of Mkt Price = {11:0.000}%
    Yearly Avg: Value = {12:0.00}, Pct of Mkt Price = {13:0.000}%

";
    public override string ToString() => string.Format(
        _template,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        Days, Months, Years,
        TotalCashProvisionCount, TotalValueCash,
        DailyAverageValueCash, DailyAverageCorporateActionPrice,
        MonthlyAverageValueCash, MonthlyAverageCorporateActionPrice,
        YearlyAverageValueCash, YearlyAverageCorporateActionPrice,
        DailyAverageCashProvisionPeriod, MonthlyAverageCashProvisionPeriod, YearlyAverageCashProvisionPeriod,
        LastCashProvision, FirstCashProvision
    );

    private static decimal SafeDivision(decimal a, int b) => b == 0 ? 0 : a / b;
}
