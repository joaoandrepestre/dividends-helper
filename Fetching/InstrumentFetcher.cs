using DividendsHelper.Models;

namespace DividendsHelper.Fetching; 

public class InstrumentFetcher : BasePagedFetcher<string, SearchResult> {
    public InstrumentFetcher(HttpClient httpClient) : base(httpClient) { }
    protected override Task<PagedHttpRequest?> GetPagedRequest(string symbol, int pageNumber = 1) =>
        Task.FromResult<PagedHttpRequest?>(new PagedHttpRequest {
            RequestType = RequestType.Search,
            Company = symbol,
            PageNumber = pageNumber,
        });
}