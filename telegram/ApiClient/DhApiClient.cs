using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DividendsHelper.Models.ApiMessages;
using DividendsHelper.Models.Core;
using static System.EnvironmentVariableTarget;

namespace DividendsHelper.Telegram.ApiClient; 

public class DhApiClient {
    private const string ApiAddressName = "DH_API";
    private static string ApiAddress => Environment.GetEnvironmentVariable(ApiAddressName, Process) ??
                                        Environment.GetEnvironmentVariable(ApiAddressName, Machine) ?? "";
    
    private readonly HttpClient _httpClient;

    public DhApiClient() {
        _httpClient = new HttpClient();
        if (string.IsNullOrEmpty(ApiAddress)) {
            Console.WriteLine(
                $"ERROR - Could not retrieve api address. Remember to set the environment variable {ApiAddressName}.");
            Environment.Exit(-1);
        }
        _httpClient.BaseAddress = new Uri(ApiAddress);
    }

    private async Task<ApiResponse<TResponse>?> DoHttpRequest<TRequest, TResponse>(string endpoint, TRequest? request = null)
        where TRequest : class
        where TResponse : class {
        var content = request is null
            ? null
            : new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var res = await _httpClient.PostAsync(endpoint, content);
        res.EnsureSuccessStatusCode();
        try {
            return await res.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
        }
        catch (JsonException) {
            return null;
        }
    }

    private Task<ApiResponse<TResponse>?> DoHttpRequest<TResponse>(string endpoint) where TResponse : class =>
        DoHttpRequest<object, TResponse>(endpoint);

    public async Task<HashSet<string>> GetMonitoredSymbols() {
        var res = await DoHttpRequest<HashSet<string>>("/instruments/monitored");
        if (res is null || res.Content is null) return new();
        return res.Content;
    }

    public async Task<CashProvisionSummary?> Monitor(string symbol) {
        var req = new Instrument {
            Symbol = symbol,
        };
        var res = await DoHttpRequest<Instrument, CashProvisionSummary>("/instruments/monitor", req);
        if (res is null) return null;
        return res.Content;
    }

    public async Task<CashProvisionSummary?> GetSummary(string symbol, DateTime minDate, DateTime maxDate) {
        var req = new ApiRequest {
            Symbol = symbol,
            MinDate = minDate,
            MaxDate = maxDate,
        };
        var res = await DoHttpRequest<ApiRequest, CashProvisionSummary>("/cash-provisions/summary", req);
        if (res is null) return null;
        return res.Content;
    }

    public async Task<Simulation?> Simulate(string symbol, decimal investment, DateTime minDate, DateTime maxDate) {
        var req = new ApiRequest {
            Symbol = symbol,
            Investment = investment,
            MinDate = minDate,
            MaxDate = maxDate,
        };
        var res = await DoHttpRequest<ApiRequest, Simulation>("/cash-provisions/simulation", req);
        if (res is null) return null;
        return res.Content;
    }

    public async Task<Portfolio?> BuildPortfolio(decimal limit, decimal investment, DateTime minDate, DateTime maxDate) {
        var req = new ApiRequest {
            QtyLimit = limit,
            Investment = investment,
            MinDate = minDate,
            MaxDate = maxDate,
        };
        var res = await DoHttpRequest<ApiRequest, Portfolio>("/cash-provisions/portfolio", req);
        if (res is null) return null;
        return res.Content;
    }
}