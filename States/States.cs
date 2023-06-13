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

    public override CashProvision Insert(CashProvision value) {
        var v = base.Insert(value);
        if (v != value) return value;
        lock (_locker) {
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
            .OrderBy(i => i.ReferenceDate);

        foreach (var p in provisions) {
            summary.TotalCashProvisionCount++;
            summary.TotalValueCash += p.ValueCash;
            summary.TotalCorporateActionPrice += p.CorporateActionPrice;

            summary.FirstCashProvision ??= p;
            summary.LastCashProvision = p;
        }

        return summary;
    }
}



public class State {
    private static readonly string _pathName = "DH_FILES";
    private static string Path => Environment.GetEnvironmentVariable(_pathName, Process) ??
        Environment.GetEnvironmentVariable(_pathName, Machine) ?? "";
    public InstrumentState Instruments { get; set; }
    public CashProvisionState CashProvisions { get; set; }

    public HashSet<string> MonitoredSymbols { get; set; } = new();

    public State() {
        Instruments = new InstrumentState(new InstrumentFetcher());
        var provisionFetcher = new CashProvisionFetcher(Instruments);
        CashProvisions = new CashProvisionState(provisionFetcher);
    }

    public async Task Load() {
        Console.WriteLine("Loading states...");
        var p = System.IO.Path.Join(Path, "monitored");
        if (!File.Exists(p)) return;
        string? s = "";
        using (var reader = new StreamReader(p)) {
            s = await reader.ReadLineAsync();
        }
        if (s == null) {
            Console.WriteLine("Loading states done.");
            return;
        }
        MonitoredSymbols.UnionWith(s.Split(",").ToHashSet());
        await Instruments.Load(MonitoredSymbols);
        await CashProvisions.Load(MonitoredSymbols);
        Console.WriteLine("Loading states done.");
    }

    public async Task Stop() {
        var s = string.Join(",", MonitoredSymbols);
        var p = System.IO.Path.Join(Path, "monitored");
        using var writer = new StreamWriter(p);
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