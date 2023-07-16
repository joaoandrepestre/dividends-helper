using System.Text.Json;
using DividendsHelper.Core.Utils;
using DividendsHelper.Models.Fetching;

namespace DividendsHelper.Core.Fetching;

public interface IBaseFetcher<TRequest, TResponse> {
    Task<IEnumerable<TResponse>> Fetch(TRequest request);
}

public abstract class BaseFetcher<TRequest, TResponse> where TResponse : class {
    private readonly HttpClient _httpClient;
    
    protected BaseFetcher(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    protected abstract string GetUrl(TRequest request);
    protected async Task<TResponse?> DoHttpRequest(TRequest request)
    {
        var url = GetUrl(request);
        var res = await _httpClient.GetAsync(url);
        res.EnsureSuccessStatusCode();
        try {
            return await res.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (JsonException) {
            return null;
        }
    }

}
public abstract class BasePagedFetcher<TRequest, TResponse> : 
    BaseFetcher<PagedHttpRequest, PagedHttpResponse<TResponse>>, 
    IBaseFetcher<TRequest, TResponse> {
    
    protected BasePagedFetcher(HttpClient httpClient) : base(httpClient) { }
    protected override string GetUrl(PagedHttpRequest request) => request.GetUrl();
    
    protected abstract Task<PagedHttpRequest?> GetPagedRequest(TRequest request, int pageNumber = 1);

    public async Task<IEnumerable<TResponse>> Fetch(TRequest request) {
        var req = await GetPagedRequest(request);
        if (req == null) return Enumerable.Empty<TResponse>();

        var pagedResponse = await DoHttpRequest(req);

        var totalPages = pagedResponse?.Page.TotalPages ?? 0;
        var ret = pagedResponse?.Results.ToList() ?? new List<TResponse>();
        for (var pageNumber = 2; pageNumber <= totalPages; pageNumber++) {
            req = await GetPagedRequest(request, pageNumber);
            if (req == null) continue;
            pagedResponse = await DoHttpRequest(req);
            ret.AddRange(pagedResponse?.Results ?? Enumerable.Empty<TResponse>());
        }

        return ret;
    }
}

public abstract class BaseUnpagedFetcher<TRequest, TResponse> : 
    BaseFetcher<UnpagedHttpRequest, UnpagedHttpResponse>,
    IBaseFetcher<TRequest, TResponse> where TResponse : class, new() {
    
    protected BaseUnpagedFetcher(HttpClient httpClient) : base(httpClient) { }

    protected override string GetUrl(UnpagedHttpRequest request) => request.GetUrl();
    protected abstract UnpagedHttpRequest? GetUnpagedRequest(TRequest request);

    public async Task<IEnumerable<TResponse>> Fetch(TRequest request)
    {
        var req = GetUnpagedRequest(request);
        if (req == null) return Enumerable.Empty<TResponse>();

        var response = await DoHttpRequest(req);
        
        return response?.ParseResponse<TResponse>() ?? 
               Enumerable.Empty<TResponse>();
    }
}