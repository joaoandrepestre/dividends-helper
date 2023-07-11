using System.Text;
using DividendsHelper.Utils;

namespace DividendsHelper.Models;

public readonly record struct CashProvisionId(string Symbol, DateTime ReferenceDate, string CorporateAction);
public class CashProvision : IBaseModel<CashProvisionId> {
    public CashProvisionId Id => new(Symbol, ReferenceDate, CorporateAction);
    public string Symbol { get; init; } = "";
    public DateTime ReferenceDate { get; init; } = DateTime.Today;
    public decimal ValueCash { get; init; }
    public decimal CorporateActionPrice { get; init; }
    public string CorporateAction { get; set; } = "";
    private decimal? _price;

    public decimal Price {
        get => _price ?? (ValueCash / CorporateActionPrice) * 100; 
        set => _price = value;
    }
    
    public override string ToString() => $"{Symbol} | {ReferenceDate.DateString()}: Value = {ValueCash:0.00} R$/unt, Pct of Mkt Price = {CorporateActionPrice:0.000}%";

}

public class CashProvisionSummary {
    // Key
    public string Symbol { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    
    // Values
    public CashProvision[] CashProvisions { get; set; } = Array.Empty<CashProvision>();
    public decimal[] IntervalsInDays { get; set; } = Array.Empty<decimal>();

    private int Days => FirstCashProvisionDate.DaysUntil(LastCashProvisionDate);
    private int Months => FirstCashProvisionDate.MonthsUntil(LastCashProvisionDate);
    private int Years => FirstCashProvisionDate.YearsUntil(LastCashProvisionDate);

    private int TotalCashProvisionCount => CashProvisions.Length;

    // Stats
    private Stats ValueCashStats => new Stats(CashProvisions.Select(c => c.ValueCash));

    private Stats CorporateActionCashStats => new Stats(CashProvisions.Select(c => c.CorporateActionPrice));

    private Stats DateIntervalStats => new Stats(IntervalsInDays);

    private CashProvision? FirstCashProvision => CashProvisions.FirstOrDefault();
    private DateTime FirstCashProvisionDate => FirstCashProvision?.ReferenceDate ?? default;
    private CashProvision? LastCashProvision => CashProvisions.LastOrDefault();
    private DateTime LastCashProvisionDate => LastCashProvision?.ReferenceDate ?? default;
    private decimal DailyAverageCashProvisionPeriod => DateIntervalStats.Average;
    private decimal MonthlyAverageCashProvisionPeriod => DateIntervalStats.Average / 30;
    private decimal YearlyAverageCashProvisionPeriod => DateIntervalStats.Average / 365;

    private const string Template = @"Summary for {0} from {1} to {2}:
    Last: {3}
    First: {4}
    
    Days = {5}, Months = {6}, Years = {7}
    Period: {8:0.00} days // {9:0.00} months // {10:0.00} years

    Cash provisions payed = {11}, Total value payed = {12:0.00} R$/unt

    Value: Avg = {13:0.00} R$/unt, σ = {14:0.00}%, Mode = {15:0.00}
    Value / Price: Avg = {16:0.00} R$/unt, σ = {17:0.00}%, Mode = {18:0.00}

";

    public override string ToString() => string.Format(
        Template,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        LastCashProvision, FirstCashProvision,
        Days, Months, Years,
        DailyAverageCashProvisionPeriod, MonthlyAverageCashProvisionPeriod, YearlyAverageCashProvisionPeriod,
        TotalCashProvisionCount, ValueCashStats.Total,
        ValueCashStats.Average,ValueCashStats.StandardDeviation,ValueCashStats.Mode,
        CorporateActionCashStats.Average, CorporateActionCashStats.StandardDeviation, CorporateActionCashStats.Mode
        );

    private const string MarkdownTemplate = @"Summary for *{0}* from _{1}_ to _{2}_
`------------------------------------------------------------------`

\- *Last*: `{3}`
\- *First*: `{4}`
    
*Days* \= `{5}`, Months \= `{6}`, Years \= `{7}`
*Period*: `{8:0.00}` _days_ // `{9:0.00}` _months_ // `{10:0.00}` _years_

Cash provisions payed \= `{11}`, Total value payed \= `{12:0.00} R$\/unt`

```
|###################| Average |   σ   |  Mode |
|-------------------|---------|-------|-------|
| Value (R$/unt)    |   {13:00.00} | {14:00.00} | {15:00.00} |
| Value / Price (%) |   {16:00.00} | {17:00.00} | {18:00.00} |
```
`------------------------------------------------------------------`

Source: [B3](https://www.b3.com.br/pt_br/produtos-e-servicos/negociacao/renda-variavel/empresas-listadas.htm)
";

    public string ToMarkdown() => string.Format(
        MarkdownTemplate,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        LastCashProvision, FirstCashProvision,
        Days, Months, Years,
        DailyAverageCashProvisionPeriod, MonthlyAverageCashProvisionPeriod, YearlyAverageCashProvisionPeriod,
        TotalCashProvisionCount, ValueCashStats.Total,
        ValueCashStats.Average,ValueCashStats.StandardDeviation,ValueCashStats.Mode,
        CorporateActionCashStats.Average, CorporateActionCashStats.StandardDeviation, CorporateActionCashStats.Mode
    );

    private static decimal SafeDivision(decimal a, int b) => b == 0 ? 0 : a / b;
}

public abstract class Investment {
    public decimal InitialInvestment { get; set; }
    public abstract decimal ResultMoney { get; }
    protected abstract int Period { get; }
    protected decimal EffectiveInterestRate => InitialInvestment == 0 ? 0 : ((ResultMoney / InitialInvestment) - 1)*100;
    protected decimal YearlyPctInterestRate => InitialInvestment == 0 ? 0 : EffectiveInterestRate.ConvertInterestRate(Period, 365);
}

public class Simulation : Investment{
    // Key
    public string Symbol { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public CashProvision[] CashProvisions { get; set; } = Array.Empty<CashProvision>();

    public DateTime FirstDate { get; set; } 
    public decimal? FirstPrice { get; set; }
    public decimal PositionQty => Math.Floor(InitialInvestment / (FirstPrice ?? 1));
    public decimal FirstPositionValue => PositionQty * (FirstPrice ?? 0);
    public decimal RemainingCash => InitialInvestment - FirstPositionValue;
    
    public DateTime FinalDate { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal LastPositionValue => PositionQty * (FinalPrice ?? 1);

    public decimal TotalDividends => CashProvisions.Sum(i => i.ValueCash) * PositionQty;
    public override decimal ResultMoney => LastPositionValue + TotalDividends;

    protected override int Period => FirstDate.DaysUntil(FinalDate);

    private const string MarkdownTemplate = @"Simulation for *{0}* from _{1}_ to _{2}_
`------------------------------------------------------------------`

*Initial investment*: `R${3:.00}`
Position _{4}_: `{5} stocks @ R${6:.00} | R${7:.00}`

Position _{8}_: `{9} stocks @ R${10:.00} | R${11:.00}`
Dividends received: `R${12:.00}`

*Results*: `R${13:.00} | {14:.000}% | {15:.000}% a.a.` 
`------------------------------------------------------------------`
";
    public string ToMarkdown() => string.Format(
        MarkdownTemplate,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        InitialInvestment,
        FirstDate.DateString(), PositionQty, FirstPrice, FirstPositionValue,
        FinalDate.DateString(), PositionQty, FinalPrice, LastPositionValue,
        TotalDividends,
        ResultMoney, EffectiveInterestRate, YearlyPctInterestRate);

    private const string PositionMarkdownTemplate = @"| {0} | {1} | {2:.00} | {3} | {4:.00} |";
    public string InitialPositionMarkdown() => string.Format(
        PositionMarkdownTemplate,
        FirstDate.DateString(), Symbol, FirstPrice, PositionQty, FirstPositionValue);
    public string FinalPositionMarkdown() => string.Format(
        PositionMarkdownTemplate,
        FinalDate.DateString(), Symbol, FinalPrice, PositionQty, LastPositionValue);

    private const string DividendsMarkdownTemplate = @"| {0} | {1} |";
    public string DividendsMarkdown() => string.Format(
        DividendsMarkdownTemplate,
        Symbol, TotalDividends);


}

public class Portfolio : Investment {
    public string[] Symbols { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public Dictionary<string, Simulation> Simulations { get; set; }
    private decimal TotalQty => Simulations.Sum(i => i.Value.PositionQty);
    private decimal InitialValue => Simulations.Sum(i => i.Value.FirstPositionValue);
    private decimal FinalValue => Simulations.Sum(i => i.Value.LastPositionValue);
    private decimal TotalDividends => Simulations.Sum(i => i.Value.TotalDividends);
    public override decimal ResultMoney => Simulations.Sum(i => i.Value.ResultMoney);

    protected override int Period => StartDate.DaysUntil(EndDate);

    private string InitialPositionTable() {
        var sb = new StringBuilder();
        foreach (var (_, s) in Simulations) {
            if (s.PositionQty > 0)
                sb.AppendLine(s.InitialPositionMarkdown());
        }
        return sb.ToString();
    }

    private string FinalPositionTable() {
        var sb = new StringBuilder();
        foreach (var (_, s) in Simulations) {
            if (s.PositionQty > 0)
                sb.AppendLine(s.FinalPositionMarkdown());
        }
        return sb.ToString();   
    }
    private string DividendsTable() {
        var sb = new StringBuilder();
        foreach (var (_, s) in Simulations) {
            if (s.PositionQty > 0)
                sb.AppendLine(s.DividendsMarkdown());
        }
        return sb.ToString(); 
    }

    private const string MarkdownTemplate = @"Portfolio from _{0}_ to _{1}_
`------------------------------------------------------------------`
*Initial investment*: `R${2:.00}`
*Initial Positions:*
```
| Date | Symbol | Price | Qty | Value |
{3}
Total: {4} stocks | R${5:.00}
```

*Final Positions:*
```
| Date | Symbol | Price (R$) | Qty | Value (R$) |
{6}
Total: {7} stocks | R${8:.00}
```

*Dividends received:*
```
| Symbol | Dividends |
{9}
Total: {10}
```

*Results*: `R${11:.00} | {12:.000}% | {13:.000}% a.a.` 
`------------------------------------------------------------------`
";

    public string ToMarkdown() => string.Format(
        MarkdownTemplate,
        StartDate.DateString(), EndDate.DateString(),
        InitialInvestment,
        InitialPositionTable(),
        TotalQty, InitialValue,
        FinalPositionTable(),
        TotalQty, FinalValue,
        DividendsTable(),
        TotalDividends,
        ResultMoney, EffectiveInterestRate, YearlyPctInterestRate
        );
}