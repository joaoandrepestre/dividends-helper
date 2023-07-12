using DividendsHelper.Models;

namespace DividendsHelper.Fetching;

public class TradingDataFetcher : BaseUnpagedFetcher<SymbolDate, TradingDataResult> {
    public TradingDataFetcher(HttpClient httpClient) : base(httpClient) { }
    protected override UnpagedHttpRequest? GetUnpagedRequest(SymbolDate request) => new() {
        RequestType = RequestType.TradingData,
        Params = new[] {
            request.Symbol, 
            $"{request.ReferenceDate.Year:0000}-{request.ReferenceDate.Month:00}-{request.ReferenceDate.Day:00}"
        },
    };
}
