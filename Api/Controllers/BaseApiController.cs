using DividendsHelper.Models;
using DividendsHelper.States;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[ApiController]
public abstract class BaseApiController<TId, T> : ControllerBase
    where T : IBaseModel<TId> 
    where TId : notnull {
    private readonly IBaseState<TId, T> _state;
    protected BaseApiController(IBaseState<TId, T> state) {
        _state = state;
    }

    [HttpPost]
    public Task<T?> Get([Bind("Id")] T f) => _state.Get(f.Id);
}