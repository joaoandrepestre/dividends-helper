using DividendsHelper.Models;
using DividendsHelper.States;

namespace DividendsHelper.Fetching;

public class InstrumentFetcher : BaseFetcher<string, SearchResult> {
    protected override PagedHttpRequest? GetPagedRequest(string symbol, int pageNumber = 1) =>
        new PagedHttpRequest {
            RequestType = RequestType.Search,
            Company = symbol,
            PageNumber = pageNumber,
        };
}
public class CashProvisionFetcher : BaseFetcher<string, CashProvisionsResult> {

    // Dependency
    private InstrumentState _instruments;

    public CashProvisionFetcher(InstrumentState instrumentState) : base() {
        _instruments = instrumentState;
    }

    protected override PagedHttpRequest? GetPagedRequest(string symbol, int pageNumber = 1) {
        var instrument = _instruments.Get(symbol);

        if (instrument == null) return null;

        return new PagedHttpRequest {
            RequestType = RequestType.CashProvisions,
            TradingName = instrument.TradingName,
            Company = symbol,
            PageNumber = pageNumber,
        };
    }
}
