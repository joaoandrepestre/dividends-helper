using DividendsHelper.Models;
using DividendsHelper.States;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[Route("cash-provisions/[action]")]
public class CashProvisionController : BaseApiController<CashProvisionId, CashProvision> {
    private readonly CashProvisionState _cashProvisions;

    public CashProvisionController(CashProvisionState state) : base(state) {
        _cashProvisions = state;
    }

    [HttpPost]
    public CashProvisionSummary Summary([Bind("Symbol,MinDate,MaxDate")] ApiRequest req) =>
        _cashProvisions.GetSummary(req.Symbol, req.MinDate, req.MaxDate);

    [HttpPost]
    public Task<Simulation> Simulation([Bind("Symbol,MinDate,MaxDate,Investment")]ApiRequest req) =>
        _cashProvisions.Simulate(req.Symbol, req.MinDate, req.MaxDate, req.Investment);

    [HttpPost]
    public Task<Portfolio> Portfolio([Bind("Symbols,MinDate,MaxDate,Investment,QtyLimit")]ApiRequest req) =>
        _cashProvisions.BuildPortfolio(req.Symbols, req.MinDate, req.MaxDate, req.Investment, req.QtyLimit);
}