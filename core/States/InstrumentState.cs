using Beef.Fetchers;
using Beef.Types.Responses;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States; 

public class InstrumentState : BaseState<string, Instrument, string, CompanySearchResponse> {
    private readonly InstrumentFetcher _fetcher;

    public InstrumentState(InstrumentFetcher fetcher) {
        _fetcher = fetcher;
    }

    protected override Instrument ConvertDto(string symbol, CompanySearchResponse dto) =>
        new() {
            Symbol = symbol,
            TradingName = dto.TradingName,
        };

    protected override IB3Fetcher<string, CompanySearchResponse> GetFetcher() => _fetcher;
}