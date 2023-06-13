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

public class CashProvisionState : BaseState<Guid, CashProvision, string, CashProvisionsResult> {
    private readonly Dictionary<string, HashSet<CashProvision>> _cacheBySymbol = new();
    private readonly Dictionary<(string, DateTime), HashSet<CashProvision>> _cacheBySymbolDate = new();

    private readonly CashProvisionFetcher _fetcher;

    public CashProvisionState(CashProvisionFetcher fetcher) {
        _fetcher = fetcher;
    }

    protected override IBaseFetcher<string, CashProvisionsResult> GetFetcher() => _fetcher;

    protected override CashProvision ConvertDto(string symbol, CashProvisionsResult dto) =>
        new() {
            Symbol = symbol,
            ReferenceDate = dto.LastDateTimePriorEx,
            ValueCash = dto.ValueCash ?? 0,
            CorporateActionPrice = dto.CorporateActionPrice ?? 0,
            CorporateAction = dto.CorporateAction,
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
        foreach (var p in provisions)
        {
            
            summary.TotalValueCash += p.ValueCash;
            summary.TotalCorporateActionPrice += p.CorporateActionPrice;
        }

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
}



public class State {
    private const string PathName = "DH_FILES";

    private static string Path => Environment.GetEnvironmentVariable(PathName, Process) ??
                                  Environment.GetEnvironmentVariable(PathName, Machine) ?? "";
    
    private InstrumentState Instruments { get; }
    public CashProvisionState CashProvisions { get; }
    private Timer? ProvisionsSyncer { get; set; }

    public HashSet<string> MonitoredSymbols { get; } = new();

    public State() {
        Instruments = new InstrumentState(new InstrumentFetcher());
        var provisionFetcher = new CashProvisionFetcher(Instruments);
        CashProvisions = new CashProvisionState(provisionFetcher);
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
        if (!instruments.Any() && !provisions.Any()) return false;
        MonitoredSymbols.Add(s);
        return true;
    }
}