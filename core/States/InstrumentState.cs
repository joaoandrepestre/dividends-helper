using DividendsHelper.Core.Fetching;
using DividendsHelper.Models.Core;
using DividendsHelper.Models.Fetching;

namespace DividendsHelper.Core.States; 

public class InstrumentState : BaseState<string, Instrument, string, SearchResult> {
    private readonly InstrumentFetcher _fetcher;

    public InstrumentState(InstrumentFetcher fetcher) {
        _fetcher = fetcher;
    }

    protected override Instrument ConvertDto(string symbol, SearchResult dto) =>
        new() {
            Symbol = symbol,
            TradingName = dto.TradingName,
        };

    protected override IBaseFetcher<string, SearchResult> GetFetcher() => _fetcher;
}