using DividendsHelper.Models;
using DividendsHelper.States;

namespace DividendsHelper.Fetching;

public class InstrumentFetcher : BasePagedFetcher<string, SearchResult> {
    protected override Task<PagedHttpRequest?> GetPagedRequest(string symbol, int pageNumber = 1) =>
        Task.FromResult<PagedHttpRequest?>(new PagedHttpRequest {
            RequestType = RequestType.Search,
            Company = symbol,
            PageNumber = pageNumber,
        });
}
public class CashProvisionFetcher : BasePagedFetcher<string, CashProvisionsResult> {

    // Dependency
    private InstrumentState _instruments;

    public CashProvisionFetcher(InstrumentState instrumentState) : base() {
        _instruments = instrumentState;
    }

    protected override async Task<PagedHttpRequest?> GetPagedRequest(string symbol, int pageNumber = 1) {
        var instrument = await _instruments.Get(symbol);

        if (instrument == null) return null;

        return new PagedHttpRequest {
            RequestType = RequestType.CashProvisions,
            TradingName = instrument.TradingName,
            Company = symbol,
            PageNumber = pageNumber,
        };
    }
}

public class TradingDataFetcher : BaseUnpagedFetcher<SymbolDate, TradingDataResult> {
    protected override UnpagedHttpRequest? GetUnpagedRequest(SymbolDate request) => new() {
        RequestType = RequestType.TradingData,
        Params = new[] {
            request.Symbol, 
            $"{request.ReferenceDate.Year:0000}-{request.ReferenceDate.Month:00}-{request.ReferenceDate.Day:00}"
        },
    };
}
