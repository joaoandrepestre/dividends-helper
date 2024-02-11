using Beef.Types.Requests;
using DividendsHelper.Core.States;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers;

public class TradingDataConfig : ControllerConfig { }

[Route("trading-data/[action]")]
public class TradingDataController : BaseApiController<SymbolDate, TradingData> {
    public TradingDataController(TradingDataState state, TradingDataConfig config) : base(state, config) { }
}