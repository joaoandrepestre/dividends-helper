﻿using DividendsHelper.Utils;

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