using DividendsHelper.Models;
using DividendsHelper.States;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[Route("trading-data/[action]")]
public class TradingDataController : BaseApiController<SymbolDate, TradingData> {
    public TradingDataController(TradingDataState state) : base(state) { }
}