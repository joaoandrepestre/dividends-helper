using DividendsHelper.Core.States;
using DividendsHelper.Models.ApiMessages;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers;

public class InstrumentConfig : ControllerConfig {
}

[Route("instruments/[action]")]
public class InstrumentController : BaseApiController<string, Instrument> {
    private readonly CoreState _coreState;

    public InstrumentController(CoreState coreState, InstrumentState state, InstrumentConfig config) : base(state, config) {
        _coreState = coreState;
    }

    [HttpPost]
    public Task<ApiResponse<string>> Monitored() {
        var action = (object _) => {
            var content = string.Join(',', _coreState.MonitoredSymbols);
            var fb = "";
            if (string.IsNullOrEmpty(content)) fb = "There are no monitored instruments";
            return Task.FromResult((content, fb));
        };
        return BaseAction(action, null);
    }

    [HttpPost]
    public Task<ApiResponse<CashProvisionSummary>> Monitor([Bind("Symbol")] Instrument i) {
        var action = async (Instrument i) => {
            CashProvisionSummary content = null;
            var fb = $"Could not find instrument {i.Symbol}";
            if (await _coreState.Monitor(i.Symbol)) {
                content = _coreState.CashProvisions.GetSummary(i.Symbol, DateTime.Today.AddYears(-1));
                fb = "";
            }
            return (content, fb);
        };
        return BaseAction(action, i);
    }
}