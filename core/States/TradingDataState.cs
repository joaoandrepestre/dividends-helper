using DividendsHelper.Core.Fetching;
using DividendsHelper.Models.Core;
using DividendsHelper.Models.Fetching;

namespace DividendsHelper.Core.States; 

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