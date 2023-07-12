using DividendsHelper.Fetching;
using DividendsHelper.Models;
using DividendsHelper.Utils;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.States;
public class CoreState {
    private const string PathName = "DH_FILES";

    private static string Path => Environment.GetEnvironmentVariable(PathName, Process) ??
                                  Environment.GetEnvironmentVariable(PathName, Machine) ?? "";
    
    private InstrumentState Instruments { get; }
    public CashProvisionState CashProvisions { get; }
    private TradingDataState TradingData { get; }
    public HashSet<string> MonitoredSymbols { get; } = new();

    public CoreState(InstrumentState instruments, CashProvisionState cashProvisions, TradingDataState tradingData) {
        Instruments = instruments;
        CashProvisions = cashProvisions;
        TradingData = tradingData;
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
        Logger.Log("Loading states done.");
    }

    public async Task Stop() {
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