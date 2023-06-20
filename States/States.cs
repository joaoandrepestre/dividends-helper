using System.Diagnostics;
using DividendsHelper.Fetching;
using DividendsHelper.Models;
using DividendsHelper.Utils;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.States;
public class InstrumentState : BaseState<string, Instrument, string, SearchResult> {
    private readonly InstrumentFetcher _fetcher;

    public InstrumentState(InstrumentFetcher fetcher) {
        _fetcher = fetcher;
    }

    protected override Instrument ConvertDto(string symbol, SearchResult dto) =>
        new() {
            Symbol = symbol,
            TradingName = dto.TradingName,
        };

    protected override IBaseFetcher<string, SearchResult> GetFetcher() => _fetcher;
}

public class CashProvisionState : BaseState<CashProvisionId, CashProvision, string, CashProvisionsResult> {
    private readonly Dictionary<string, HashSet<CashProvision>> _cacheBySymbol = new();
    private readonly Dictionary<(string, DateTime), HashSet<CashProvision>> _cacheBySymbolDate = new();

    private readonly CashProvisionFetcher _fetcher;
    private readonly TradingDataState _tradingData;

    public CashProvisionState(CashProvisionFetcher fetcher, TradingDataState tradingData) {
        _fetcher = fetcher;
        _tradingData = tradingData;
    }

    protected override IBaseFetcher<string, CashProvisionsResult> GetFetcher() => _fetcher;

    protected override CashProvision ConvertDto(string symbol, CashProvisionsResult dto) =>
        new() {
            Symbol = symbol,
            ReferenceDate = dto.LastDateTimePriorEx,
            ValueCash = dto.ValueCash ?? 0,
            CorporateActionPrice = dto.CorporateActionPrice ?? 0,
            CorporateAction = dto.CorporateAction,
            Price = dto.ClosingPricePriorExDate ?? 0,
        };

    protected override CashProvision Insert(CashProvision value) {
        var v = base.Insert(value);
        if (v != value) return value;
        lock (Locker) {
            _cacheBySymbol.GetOrAdd(value.Symbol).Add(value);
            _cacheBySymbolDate.GetOrAdd((value.Symbol, value.ReferenceDate)).Add(value);
        }
        return v;
    }

    public override IEnumerable<CashProvision> Insert(string symbol, IEnumerable<CashProvisionsResult> dtos) {
        var grouped = dtos
            .GroupBy(dto => new CashProvisionId(symbol, dto.LastDateTimePriorEx, dto.CorporateAction));
        var consolidated = new List<CashProvision>();
        foreach (var g in grouped) {
            consolidated.Add(new CashProvision {
                Symbol = g.Key.Symbol,
                ReferenceDate = g.Key.ReferenceDate,
                CorporateAction = g.Key.CorporateAction,
                Price = g.FirstOrDefault()?.ClosingPricePriorExDate ?? 0,
                ValueCash = g.Sum(i => i.ValueCash ?? 0),
                CorporateActionPrice = g.Sum(i => i.CorporateActionPrice ?? 0),
            });
        }
        return Insert(consolidated);
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

    public async Task<Simulation> Simulate(string symbol, DateTime minDate, DateTime maxDate, decimal investment) {
        var simulation = new Simulation(investment) {
            Symbol = symbol,
            StartDate = minDate,
            EndDate = maxDate,
        };
        if (!_cacheBySymbol.TryGetValue(symbol, out var values)) return simulation;
        
        var provisions = values
            .Where(i => i.ReferenceDate >= minDate)
            .Where(i => i.ReferenceDate <= maxDate)
            .OrderBy(i => i.ReferenceDate)
            .ToArray();

        simulation.CashProvisions = provisions;
        // Find first available trading data
        TradingData? data = null;
        var date = minDate == DateTime.MinValue ? provisions.First().ReferenceDate : minDate;
        var oldestDate = DateTime.Today.AddDays(-20); // oldest trading data available at B3 website
        if (date < oldestDate) {
            // get price from cash provisions
            var firstProvision = provisions.First();
            simulation.FirstDate = firstProvision.ReferenceDate;
            simulation.FirstPrice = firstProvision.Price;
        }
        else {
            while (data == null)
            {
                data = await _tradingData.Get(new SymbolDate(symbol, date));
                date = date.AddDays(1);
            }

            simulation.FirstDate = data.ReferenceDate;
            simulation.FirstPrice = data.ClosingPrice;
        }

        // Find last available trading data
        date = maxDate;
        if (date < oldestDate) {
            // get price from cash provisions
            var lastProvision = provisions.Last();
            simulation.FinalDate = lastProvision.ReferenceDate;
            simulation.FinalPrice = lastProvision.Price;
        }
        else
        {
            data = null;
            while (data == null)
            {
                data = await _tradingData.Get(new SymbolDate(symbol, date));
                date = date.AddDays(-1);
            }

            simulation.FinalDate = data.ReferenceDate;
            simulation.FinalPrice = data.ClosingPrice;
        }

        return simulation;
    }
}

public class TradingDataState : BaseState<SymbolDate, TradingData, SymbolDate, TradingDataResult> {
    
    private readonly TradingDataFetcher _fetcher;

    public TradingDataState(TradingDataFetcher fetcher) {
        _fetcher = fetcher;
    }

    protected override IBaseFetcher<SymbolDate, TradingDataResult> GetFetcher() => _fetcher;

    protected override TradingData ConvertDto(SymbolDate req, TradingDataResult dto) => new() {
        Symbol = req.Symbol,
        ReferenceDate = req.ReferenceDate,
        ClosingPrice = dto.Price,
    };

    public override async Task<TradingData?> Get(SymbolDate id) {
        var ret = await base.Get(id);
        if (ret != null) return ret;
        if (await FetchAndInsert(id) == 0) return null;
        return await base.Get(id);
    }

    protected override async Task<int> FetchAndInsert(SymbolDate request) {
        var res = await GetFetcher().Fetch(request);
        if (res is null || !res.Any()) return 0;
        var recent = res.MaxBy(i => i.TradingDateTime);
        var ret = Insert(request, recent);
        return 1;
    }
}
public class State {
    private const string PathName = "DH_FILES";

    private static string Path => Environment.GetEnvironmentVariable(PathName, EnvironmentVariableTarget.Process) ??
                                  Environment.GetEnvironmentVariable(PathName, Machine) ?? "";
    
    private InstrumentState Instruments { get; }
    public CashProvisionState CashProvisions { get; }
    private TradingDataState TradingData { get; }
    private Timer? ProvisionsSyncer { get; set; }

    public HashSet<string> MonitoredSymbols { get; } = new();

    public State() {
        Instruments = new InstrumentState(new InstrumentFetcher());
        TradingData = new TradingDataState(new TradingDataFetcher());
        var provisionFetcher = new CashProvisionFetcher(Instruments);
        CashProvisions = new CashProvisionState(provisionFetcher, TradingData);
    }

    public async Task Load() {
        Logger.Log("Loading states...");
        var p = System.IO.Path.Join(Path, "monitored");
        if (!File.Exists(p)) return;
        string? s = "";
        using (var reader = new StreamReader(p)) {
            s = await reader.ReadLineAsync();
        }
        if (s == null) {
            Logger.Log("Loading states done.");
            return;
        }
        MonitoredSymbols.UnionWith(s.Split(",").ToHashSet());
        await Instruments.Load(MonitoredSymbols);
        await CashProvisions.Load(MonitoredSymbols);
        await TradingData.Load(MonitoredSymbols
            .Select(symbol => new SymbolDate(symbol, DateTime.Today.AddDays(-1)))
        );
        ProvisionsSyncer = new Timer(new TimerCallback(async _ =>
        {
            await Task.WhenAll(MonitoredSymbols.Select(CashProvisions.Fetch));
        }), null, 30 * 1000, 30 * 60 * 1000);
        
        Logger.Log("Loading states done.");
    }

    public async Task Stop() {
        if (ProvisionsSyncer is not null)
            await ProvisionsSyncer.DisposeAsync();
        var s = string.Join(",", MonitoredSymbols);
        var p = System.IO.Path.Join(Path, "monitored");
        await using var writer = new StreamWriter(p);
        await writer.WriteLineAsync(s);
    }

    public async Task<bool> Monitor(string symbol) {
        var s = symbol.ToUpper();
        var instruments = await Instruments.Fetch(s);
        var provisions = await CashProvisions.Fetch(s);
        if (instruments + provisions <= 0) return false;
        MonitoredSymbols.Add(s);
        return true;
    }
}