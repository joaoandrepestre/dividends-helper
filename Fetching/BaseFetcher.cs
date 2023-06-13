using System.Net.Http.Json;
using DividendsHelper.Models;
using DividendsHelper.Utils;

namespace DividendsHelper.Fetching;

public interface IBaseFetcher<TRequest, TResponse> {
    Task<IEnumerable<TResponse>> Fetch(TRequest request);
}
public abstract class BaseFetcher<TRequest, TResponse> : IBaseFetcher<TRequest, TResponse> {
    private HttpClient _httpClient;

    public BaseFetcher() {
        _httpClient = new HttpClient();
    }

    private async Task<PagedHttpResponse<TResponse>?> DoHttpRequest(PagedHttpRequest request) {
        var url = request.GetUrl();
        var res = await _httpClient.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<PagedHttpResponse<TResponse>>();
    }

    protected abstract PagedHttpRequest? GetPagedRequest(TRequest request, int pageNumber = 1);

    public async Task<IEnumerable<TResponse>> Fetch(TRequest request) {
        var req = GetPagedRequest(request);
        if (req == null) return Enumerable.Empty<TResponse>();

        var pagedResponse = await DoHttpRequest(req);

        var totalPages = pagedResponse?.Page.TotalPages ?? 0;
        var ret = pagedResponse?.Results.ToList() ?? new List<TResponse>();
        for (var pageNumber = 2; pageNumber <= totalPages; pageNumber++) {
            req = GetPagedRequest(request, pageNumber);
            if (req == null) continue;
            pagedResponse = await DoHttpRequest(req);
            ret.AddRange(pagedResponse?.Results ?? Enumerable.Empty<TResponse>());
        }

        return ret;
    }
}