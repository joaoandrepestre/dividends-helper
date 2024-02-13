using Beef.Fetchers;
using Beef.Types.Responses;
using Crudite;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States;

public class InstrumentDtoConverter : IDtoConverter<string, CompanySearchResponse, Instrument> {
    public Instrument ConvertDto(string symbol, CompanySearchResponse dto) => new() {
        Symbol = symbol,
        TradingName = dto.TradingName,
    };
}
public class InstrumentState : BaseState<string, Instrument, string, CompanySearchResponse> {

    public InstrumentState(InstrumentLoader loader, InstrumentDtoConverter dtoConverter) : base(loader, dtoConverter) { }
}