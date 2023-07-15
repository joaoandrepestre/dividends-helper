using DividendsHelper.Core.States;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers; 

[Route("trading-data/[action]")]
public class TradingDataController : BaseApiController<SymbolDate, TradingData> {
    public TradingDataController(TradingDataState state) : base(state) { }
}