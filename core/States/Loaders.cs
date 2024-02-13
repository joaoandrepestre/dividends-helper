using Beef.Fetchers;
using Beef.Types.Requests;
using Beef.Types.Responses;
using Crudite;
using Crudite.Types;
using DividendsHelper.Core.Utils;

namespace DividendsHelper.Core.States; 

public abstract class FetchingLoader<TRequest, TLoaded> : ICrudStateLoader<TRequest, TLoaded> {
    private readonly IB3Fetcher<TRequest, TLoaded> _fetcher;
    public FetchingLoader(IB3Fetcher<TRequest, TLoaded> fetcher) {
        _fetcher = fetcher;
    }

    public async Task<IEnumerable<LoadedItems<TRequest, TLoaded>>> Load(IEnumerable<TRequest> loadRequests) {
        Logger.Log("Initial fetch...");
        var ret = new List<LoadedItems<TRequest, TLoaded>>();
        foreach (var req in loadRequests) {
            var dtos = await Fetch(req);
            ret.Add(new LoadedItems<TRequest, TLoaded>() {
                Request = req,
                Items = dtos,
            });
        }
        Logger.Log("Initial fetch done.");
        return ret;
    }

    protected virtual Task<IEnumerable<TLoaded>> DoFetch(TRequest request) =>
        _fetcher.Fetch(request);
    
    public async Task<IEnumerable<TLoaded>> Fetch(TRequest request) {
        Logger.Log($"Fetching {typeof(TLoaded).Name} data for {request}...");
        var res = await DoFetch(request);
        Logger.Log($"Fetching {typeof(TLoaded).Name} data for {request} done. Fetched {res.Count()} items.");
        return res ?? Enumerable.Empty<TLoaded>();
    }
}

public class InstrumentLoader : FetchingLoader<string, CompanySearchResponse> {
    public InstrumentLoader(InstrumentFetcher fetcher) : base(fetcher) { }
}

public class CashProvisionLoader : FetchingLoader<string, CashProvisionResponse> {
    public CashProvisionLoader(CashProvisionFetcher fetcher) : base(fetcher) { }
}

public class TradingDataLoader : FetchingLoader<SymbolDate, TradingDataResponse> {
    protected override async Task<IEnumerable<TradingDataResponse>> DoFetch(SymbolDate request) { 
        var res = await base.DoFetch(request);
        if (res is null || !res.Any()) return Enumerable.Empty<TradingDataResponse>();
        var recent = res.MaxBy(i => i.TradingDateTime);
        if (recent is null) return Enumerable.Empty<TradingDataResponse>();
        return new []{ recent };
    }

    public TradingDataLoader(TradingDataFetcher fetcher) : base(fetcher) { }
}