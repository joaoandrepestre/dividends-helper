using DividendsHelper.Core.States;
using DividendsHelper.Models.ApiMessages;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers;

public class CashProvisionConfig : ControllerConfig { }

[Route("cash-provisions/[action]")]
public class CashProvisionController : BaseApiController<CashProvisionId, CashProvision> {
    private readonly CashProvisionState _cashProvisions;
    private readonly CoreState _core;
    public CashProvisionController(CoreState core, CashProvisionState state, CashProvisionConfig config) : base(state, config) {
        _cashProvisions = state;
        _core = core;
    }

    [HttpPost]
    public Task<ApiResponse<CashProvisionSummary>> Summary([Bind("Symbol,MinDate,MaxDate")] ApiRequest req) {
        var action = (ApiRequest req) => {
            var content = _cashProvisions.GetSummary(req.Symbol, req.MinDate, req.MaxDate);
            return Task.FromResult((content, ""));
        };
        return BaseAction(action, req);
    }

    [HttpPost]
    public Task<ApiResponse<Simulation>> Simulation([Bind("Symbol,MinDate,MaxDate,Investment")] ApiRequest req) {
        var action = async (ApiRequest req) => {
            var content = await _cashProvisions.Simulate(req.Symbol, req.MinDate, req.MaxDate, req.Investment);
            var fb = "";
            if (content is null) fb = $"Could not simulate {req.Symbol} from {req.MinDate} to {req.MaxDate}";
            return (content, fb);
        };
        return BaseAction(action, req);
    }

    [HttpPost]
    public Task<ApiResponse<Portfolio>> Portfolio([Bind("Symbols,MinDate,MaxDate,Investment,QtyLimit")] ApiRequest req) {
        var action = async (ApiRequest req) => {
            var symbols = req.Symbols ?? _core.MonitoredSymbols.ToArray();
            var content = await _cashProvisions.BuildPortfolio(symbols, req.MinDate, req.MaxDate, req.Investment,
                req.QtyLimit);
            var fb = "";
            if (content is null) fb = "Could not build portfolio";
            return (content, fb);
        };
        return BaseAction(action, req);
    }
}