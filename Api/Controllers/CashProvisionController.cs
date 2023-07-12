using DividendsHelper.Models;
using DividendsHelper.States;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[Route("cash-provisions/[action]")]
public class CashProvisionController : BaseApiController<CashProvisionId, CashProvision> {
    public CashProvisionController(CashProvisionState state) : base(state) { }
}