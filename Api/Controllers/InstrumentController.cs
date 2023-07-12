using DividendsHelper.Models;
using DividendsHelper.States;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers;

[Route("instruments/[action]")]
public class InstrumentController : BaseApiController<string, Instrument> {
    private readonly CoreState _coreState;

    public InstrumentController(CoreState coreState, InstrumentState state) : base(state) {
        _coreState = coreState;
    }
    public string Monitored() =>
        string.Join(',', _coreState.MonitoredSymbols);

    [HttpPost]
    public async Task<CashProvisionSummary?> Monitor([Bind("Symbol")] Instrument i) {
        if (!await _coreState.Monitor(i.Symbol)) return null;
        return _coreState.CashProvisions.GetSummary(i.Symbol, DateTime.Today.AddYears(-1));
    }
}