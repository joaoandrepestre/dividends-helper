using DividendsHelper.Core.States;
using DividendsHelper.Models.Fetching;

namespace DividendsHelper.Core.Fetching; 

public class CashProvisionFetcher : BasePagedFetcher<string, CashProvisionsResult> {

    // Dependency
    private readonly InstrumentState _instruments;

    public CashProvisionFetcher(HttpClient httpClient, InstrumentState instrumentState) : base(httpClient) {
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