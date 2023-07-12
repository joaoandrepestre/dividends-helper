using DividendsHelper.Models;
using DividendsHelper.States;
using DividendsHelper.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[ApiController]
[Route("instruments/[action]")]
public class InstrumentController : ControllerBase {
    private readonly CoreState _coreState;

    public InstrumentController(CoreState coreState) {
        _coreState = coreState;
    }
    public string Monitored() {
        var res = string.Join(',', _coreState.MonitoredSymbols);
        Logger.Log(res);
        return res;
    }

    public async Task<CashProvisionSummary?> Monitor(string symbol) {
        if (!await _coreState.Monitor(symbol)) return null;
        return _coreState.CashProvisions.GetSummary(symbol, DateTime.Today.AddYears(-1));
    }
}