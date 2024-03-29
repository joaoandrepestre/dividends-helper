using Beef.Fetchers;
using Beef.Types.Requests;
using Beef.Types.Responses;
using Crudite;
using DividendsHelper.Core.Utils;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States;

public class CashProvisionDtoConverter : IDtoConverter<string, CashProvisionResponse, CashProvision> {
    public CashProvision ConvertDto(string symbol, CashProvisionResponse dto) =>
        new() {
            Symbol = symbol,
            ReferenceDate = dto.LastDateTimePriorEx,
            ValueCash = dto.ValueCash ?? 0,
            CorporateActionPrice = dto.CorporateActionPrice ?? 0,
            CorporateAction = dto.CorporateAction,
            Price = dto.ClosingPricePriorExDate ?? 0,
        };
}
public class CashProvisionState : BaseState<CashProvisionId, CashProvision, string, CashProvisionResponse> {
    private readonly Dictionary<string, HashSet<CashProvision>> _cacheBySymbol = new();
    private readonly Dictionary<(string, DateTime), HashSet<CashProvision>> _cacheBySymbolDate = new();

    private readonly TradingDataState _tradingData;

    public CashProvisionState(CashProvisionLoader loader, CashProvisionDtoConverter dtoConverter, 
        TradingDataState tradingData) : base(loader, dtoConverter){
        _tradingData = tradingData;
    }
    
    protected override CashProvision Create(CashProvision value) {
        var v = base.Create(value);
        if (v != value) return value;
        lock (Locker) {
            _cacheBySymbol.GetOrAdd(value.Symbol).Add(value);
            _cacheBySymbolDate.GetOrAdd((value.Symbol, value.ReferenceDate)).Add(value);
        }
        return v;
    }

    protected override Task<IEnumerable<CashProvision>> CreateLoaded(string symbol, IEnumerable<CashProvisionResponse> dtos) {
        var grouped = dtos
            .GroupBy(dto => new CashProvisionId(symbol, dto.LastDateTimePriorEx, dto.CorporateAction));
        var consolidated = new List<CashProvision>();
        foreach (var g in grouped) {
            consolidated.Add(new CashProvision {
                Symbol = g.Key.Symbol,
                ReferenceDate = g.Key.ReferenceDate,
                CorporateAction = g.Key.CorporateAction,
                Price = g.FirstOrDefault(i => i.ClosingPricePriorExDate > 0)?.ClosingPricePriorExDate ?? 0,
                ValueCash = g.Sum(i => i.ValueCash ?? 0),
                CorporateActionPrice = g.Sum(i => i.CorporateActionPrice ?? 0),
            });
        }
        return Create(consolidated);
    }

    public CashProvisionSummary GetSummary(string symbol) => GetSummary(symbol, new DateTime(1, 1, 1));

    public CashProvisionSummary GetSummary(string symbol, DateTime minDate) => GetSummary(symbol, minDate, DateTime.Today);
    public CashProvisionSummary GetSummary(string symbol, DateTime minDate, DateTime maxDate) {
        var summary = new CashProvisionSummary {
            Symbol = symbol,
            StartDate = minDate,
            EndDate = maxDate,
        };
        if (!_cacheBySymbol.TryGetValue(symbol, out var values)) return summary;

        var provisions = values
            .Where(i => i.ReferenceDate >= minDate)
            .Where(i => i.ReferenceDate <= maxDate)
            .OrderBy(i => i.ReferenceDate)
            .ToArray();

        summary.CashProvisions = provisions;
        var differentDates = provisions
            .Select(c => c.ReferenceDate)
            .Distinct();
        
        var intervals = new List<decimal>();
        var prevDate = DateTime.MinValue;
        foreach (var date in differentDates) {
            if (prevDate != DateTime.MinValue) {
                var days = (decimal) Math.Round((date-prevDate).TotalDays);
                intervals.Add(days);
            }
            prevDate = date;
        }

        summary.IntervalsInDays = intervals.ToArray();

        return summary;
    }

    private async Task<(DateTime refDate, decimal price)?> FindFirstPrice(string symbol, DateTime minDate) {
        if (!_cacheBySymbol.TryGetValue(symbol, out var values)) return null;
        var firstProvision = values
            .OrderBy(i => i.ReferenceDate)
            .First(i => i.ReferenceDate >= minDate);
        var date = minDate == DateTime.MinValue ? firstProvision.ReferenceDate : minDate;
        var oldestDate = DateTime.Today.AddDays(-20); // oldest trading data available at B3 website
        if (date < oldestDate) {
            // get price from cash provisions
            return (firstProvision.ReferenceDate, firstProvision.Price);
        }

        // check at most 1 week
        TradingData? data = null;
        for (; date <= date.AddDays(7); date = date.AddDays(1)) {
            data = await _tradingData.Read(new SymbolDate(symbol, date));
            if (data is not null) break;
        }

        if (data is null) return null;
        return (data.ReferenceDate, data.ClosingPrice);
    }

    private async Task<(DateTime refDate, decimal price)?> FindLastPrice(string symbol, DateTime maxDate) {
        if (!_cacheBySymbol.TryGetValue(symbol, out var values)) return null;
        var lastProvision = values
            .OrderByDescending(i => i.ReferenceDate)
            .First(i => i.ReferenceDate <= maxDate);
        // Find last available trading data
        var date = maxDate;
        var oldestDate = DateTime.Today.AddDays(-20); // oldest trading data available at B3 website
        if (date < oldestDate) {
            // get price from cash provisions
            return (lastProvision.ReferenceDate, lastProvision.Price);
        }
        TradingData? data = null;
        for (; date >= date.AddDays(-7); date = date.AddDays(-1)) {
            data = await _tradingData.Read(new SymbolDate(symbol, date));
            if (data is not null) break;
        }
        
        if (data is null) return null;
        return (data.ReferenceDate, data.ClosingPrice);
    } 

    public async Task<Simulation> Simulate(string symbol, DateTime minDate, DateTime maxDate, decimal investment) {
        var simulation = new Simulation {
            Symbol = symbol,
            StartDate = minDate,
            EndDate = maxDate,
            InitialInvestment = investment,
        };
        if (!_cacheBySymbol.TryGetValue(symbol, out var values)) return simulation;
        
        var provisions = values
            .Where(i => i.ReferenceDate >= minDate)
            .Where(i => i.ReferenceDate <= maxDate)
            .OrderBy(i => i.ReferenceDate)
            .ToArray();

        simulation.CashProvisions = provisions;

        var first = await FindFirstPrice(symbol, minDate);
        simulation.FirstDate = first?.refDate ?? minDate;
        simulation.FirstPrice = first?.price ?? 0;

        var last = await FindLastPrice(symbol, maxDate);
        simulation.FinalDate = last?.refDate ?? maxDate;
        simulation.FinalPrice = last?.price ?? 0;
        
        return simulation;
    }
    
    // knapsack problem
    // maximize sum(Qty * Total Value Cash) foreach symbols
    // where sum(Qty * First Price) foreach symbol <= investment
    public async Task<Portfolio> BuildPortfolio(string[] symbols, DateTime minDate, DateTime maxDate, decimal investment, decimal limit) {
        var portfolio = new Portfolio {
            Symbols = symbols,
            StartDate = minDate,
            EndDate = maxDate,
            InitialInvestment = investment,
        };
        
        // find initial price foreach symbol
        var prices = new List<decimal>();
        var totalCash = new List<decimal>();
        foreach (var symbol in symbols) {
            var p = ((await FindFirstPrice(symbol, minDate))?.price ?? 1) * 100;
            var v = ((await Simulate(symbol, minDate, maxDate, p)).ResultMoney) * 100;
            prices.Add(p);
            totalCash.Add(v);
        }
        
        // re-examine algo choice for larger input sizes
        var qty = Algorithms.Knapsack(totalCash.ToArray(), prices.ToArray(), investment * 100, limit, Algorithms.KnapsackAlgo.Greedy);
        
        portfolio.Simulations = new Dictionary<string, Simulation>();
        for (var i = 0; i < symbols.Length; i++) {
            portfolio.Simulations.Add(symbols[i], await Simulate(symbols[i], minDate, maxDate, prices[i]*qty[i] / 100));
        }

        return portfolio;
    }
}