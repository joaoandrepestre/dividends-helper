using DividendsHelper.Core.States;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers;

[ApiController]
public abstract class BaseApiController<TId, T> : ControllerBase
    where T : IBaseModel<TId> 
    where TId : notnull {
    private readonly IBaseState<TId, T> _state;
    protected BaseApiController(IBaseState<TId, T> state) {
        _state = state;
    }

    [HttpPost]
    public Task<T?> Read([Bind("Id")] T f) {
        
        return _state.Get(f.Id);
    }
}