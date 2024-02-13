using Beef.Fetchers;
using Beef.Types.Requests;
using Beef.Types.Responses;
using Crudite;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States; 

public class TradingDataDtoConverter : IDtoConverter<SymbolDate, TradingDataResponse, TradingData> {
    public TradingData ConvertDto(SymbolDate req, TradingDataResponse dto) => new() {
        Symbol = req.Symbol,
        ReferenceDate = req.ReferenceDate.ToDateTime(TimeOnly.MinValue),
        ClosingPrice = dto.Price,
    };
}

public class TradingDataState : BaseState<SymbolDate, TradingData, SymbolDate, TradingDataResponse> {
    
    public TradingDataState(TradingDataLoader loader, TradingDataDtoConverter dtoConverter) : base(loader, dtoConverter) { }
    
    public override async Task<TradingData?> Read(SymbolDate id) {
        var ret = await base.Read(id);
        if (ret != null) return ret;
        await DoLoad(new[] { id });
        return await base.Read(id);
    }
}