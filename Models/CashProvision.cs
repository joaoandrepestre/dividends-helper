using DividendsHelper.Utils;

namespace DividendsHelper.Models;

public class CashProvision : IBaseModel<Guid> {
    public Guid Id { get; }
    public string Symbol { get; init; } = "";
    public DateTime ReferenceDate { get; init; } = DateTime.Today;
    public decimal ValueCash { get; init; }
    public decimal CorporateActionPrice { get; init; }
    public string CorporateAction { get; set; } = "";
    public decimal Price { get; set; }

    public CashProvision() {
        Id = Guid.NewGuid();
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

    public int TotalCashProvisionCount => CashProvisions.Length;

    // Stats
    public Stats ValueCashStats => new Stats(CashProvisions.Select(c => c.ValueCash));
    private decimal DailyAverageValueCash => SafeDivision(ValueCashStats.Total, Days);
    private decimal MonthlyAverageValueCash => SafeDivision(ValueCashStats.Total, Months);
    private decimal YearlyAverageValueCash => SafeDivision(ValueCashStats.Total, Years);

    public Stats CorporateActionCashStats => new Stats(CashProvisions.Select(c => c.CorporateActionPrice));
    private decimal DailyAverageCorporateActionPrice => SafeDivision(CorporateActionCashStats.Total, Days);
    private decimal MonthlyAverageCorporateActionPrice => SafeDivision(CorporateActionCashStats.Total, Months);
    private decimal YearlyAverageCorporateActionPrice => SafeDivision(CorporateActionCashStats.Total, Years);

    public Stats DateIntervalStats => new Stats(IntervalsInDays);
    
    public CashProvision? FirstCashProvision => CashProvisions.FirstOrDefault();
    private DateTime FirstCashProvisionDate => FirstCashProvision?.ReferenceDate ?? default;
    public CashProvision? LastCashProvision => CashProvisions.LastOrDefault();
    private DateTime LastCashProvisionDate => LastCashProvision?.ReferenceDate ?? default;
    private decimal DailyAverageCashProvisionPeriod => DateIntervalStats.Average;
    private decimal MonthlyAverageCashProvisionPeriod => DateIntervalStats.Average / 30;
    private decimal YearlyAverageCashProvisionPeriod => DateIntervalStats.Average / 365;

    private const string Template = @"Summary for {0} from {1} to {2}:
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
        Template,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        Days, Months, Years,
        TotalCashProvisionCount, ValueCashStats.Total,
        DailyAverageValueCash, DailyAverageCorporateActionPrice,
        MonthlyAverageValueCash, MonthlyAverageCorporateActionPrice,
        YearlyAverageValueCash, YearlyAverageCorporateActionPrice,
        DailyAverageCashProvisionPeriod, MonthlyAverageCashProvisionPeriod, YearlyAverageCashProvisionPeriod,
        LastCashProvision, FirstCashProvision
    );

    private const string MarkdownTemplate = @"Summary for *{0}* from _{1}_ to _{2}_
`------------------------------------------------------------------`

\- *Last*: `{17}`
\- *First*: `{18}`
    
*Days* \= `{3}`, Months \= `{4}`, Years \= `{5}`
*Period*: `{14:0.00}` _days_ // `{15:0.00}` _months_ // `{16:0.00}` _years_

Cash provisions payed \= `{6}`, Total value payed \= `{7:0.00} R$\/unt`

```
|#########|     Value     | Pct of Mkt Price |
|---------|---------------|------------------|
| Daily   |  {8:00.00} R$/unt |          {9:00.000}% |
| Monthly |  {10:00.00} R$/unt |          {11:00.000}% |
| Yearly  |  {12:00.00} R$/unt |          {13:00.000}% |
```
`------------------------------------------------------------------`

Source: [B3](https://www.b3.com.br/pt_br/produtos-e-servicos/negociacao/renda-variavel/empresas-listadas.htm)
";

    public string ToMarkdown() => string.Format(
        MarkdownTemplate,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        Days, Months, Years,
        TotalCashProvisionCount, ValueCashStats.Total,
        DailyAverageValueCash, DailyAverageCorporateActionPrice,
        MonthlyAverageValueCash, MonthlyAverageCorporateActionPrice,
        YearlyAverageValueCash, YearlyAverageCorporateActionPrice,
        DailyAverageCashProvisionPeriod, MonthlyAverageCashProvisionPeriod, YearlyAverageCashProvisionPeriod,
        LastCashProvision, FirstCashProvision
    );

    private static decimal SafeDivision(decimal a, int b) => b == 0 ? 0 : a / b;
}

public class Simulation {
    // Key
    public string Symbol { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    private decimal InitialInvestment { get; init; }
    public CashProvision[] CashProvisions { get; set; } = Array.Empty<CashProvision>();

    private CashProvision? FirstCashProvision => CashProvisions.FirstOrDefault();
    private DateTime FirstCashProvisionDate => FirstCashProvision?.ReferenceDate ?? default;
    private decimal PositionQty => Math.Floor(InitialInvestment / (FirstCashProvision?.Price ?? 1));
    private decimal FirstPositionValue => PositionQty * (FirstCashProvision?.Price ?? 1);
    private decimal RemainingCash => InitialInvestment - FirstPositionValue;
    private CashProvision? LastCashProvision => CashProvisions.LastOrDefault();
    private DateTime LastCashProvisionDate => LastCashProvision?.ReferenceDate ?? default;
    private decimal LastPositionValue => PositionQty * (LastCashProvision?.Price ?? 1);

    private decimal TotalDividends => CashProvisions.Sum(i => i.ValueCash) * PositionQty;
    private decimal ResultMoney => LastPositionValue + TotalDividends + RemainingCash;
    private decimal EffectiveInterestRate => 100 * ((ResultMoney / InitialInvestment) - 1);
    
    

    public Simulation(decimal investment)
    {
        InitialInvestment = investment;
    }

    private const string MarkdownTemplate = @"Simulation for *{0}* from _{1}_ to _{2}_
`------------------------------------------------------------------`

*Initial investment*: `R${3:.00}`
Position @ _{4}_: `{5} stocks | R${6:.00}`

Position @ _{7}_: `{8} stocks | R${9:.00}`
Dividends received: `R${10:.00}`

*Results*: `R${11:.00} | {12:.000}%` 
`------------------------------------------------------------------`
";
    public string ToMarkdown() => string.Format(
        MarkdownTemplate,
        Symbol, StartDate.DateString(), EndDate.DateString(),
        InitialInvestment,
        FirstCashProvisionDate.DateString(), PositionQty, FirstPositionValue,
        LastCashProvisionDate.DateString(), PositionQty, LastPositionValue,
        TotalDividends,
        ResultMoney, EffectiveInterestRate);

}