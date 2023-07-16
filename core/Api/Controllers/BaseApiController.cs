using DividendsHelper.Core.States;
using DividendsHelper.Models.ApiMessages;
using DividendsHelper.Models.Core;
using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Core.Controllers;

[ApiController]
public abstract class BaseApiController<TId, T> : ControllerBase
    where T : IBaseModel<TId> 
    where TId : notnull {
    private readonly IBaseState<TId, T> _state;
    private readonly IControllerConfig _config;
    protected BaseApiController(IBaseState<TId, T> state, IControllerConfig config) {
        _state = state;
        _config = config;
    }

    protected async Task<ApiResponse<TContent>> BaseAction<TInput, TContent>(
        Func<TInput, Task<(TContent?, string feedback)>> action,
        TInput input,
        bool enabled = true) {
        var response = new ApiResponse<TContent>();
        if (!enabled) {
            response.Feedback = "Endpoint is not enabled";
            return response;
        }
        var (content, fb) = await action(input);
        if (content is null) {
            response.Feedback = fb;
            return response;
        }
        response.Success = true;
        response.Content = content;
        return response;
    }

    [HttpPost]
    public Task<ApiResponse<IEnumerable<T>>> Create(IEnumerable<T> dtos) {
        var action = (IEnumerable<T> dtos) => {
            var content = _state.Create(dtos);
            var fb = "";
            var count = dtos?.Count() ?? 0;
            if (content is null || !content.Any()) fb = $"Could not create {count} {typeof(T).Name}";
            return Task.FromResult((content, fb));
        };
        return BaseAction(action, dtos, _config.EnableCreateAction);
    }

    [HttpPost]
    public Task<ApiResponse<T>> Read([Bind("Id")] T f) {
        var action = async (T f) => {
            var content = await _state.Read(f.Id);
            var fb = "";
            if (content is null) fb = $"Could not find {typeof(T).Name} with id {f.Id}";
            return (content, fb);
        };
        return BaseAction(action, f, _config.EnableReadAction);
    }

    [HttpPost]
    public Task<ApiResponse<IEnumerable<T>>> Update(IEnumerable<T> dtos) {
        var action =  (IEnumerable<T> dtos) => {
            var content = _state.Update(dtos);
            var fb = "";
            var count = dtos?.Count() ?? 0;
            if (content is null || content.Any()) fb = $"Could not update {count} {typeof(T).Name}";
            return Task.FromResult((content, fb));
        };
        return BaseAction(action, dtos, _config.EnableUpdateAction);
    }

    [HttpPost]
    public Task<ApiResponse<T>> Delete([Bind("Id")] T f) {
        var action = async (T f) => {
            var content = await _state.Delete(f.Id);
            var fb = $"Deleted {typeof(T).Name} with id {f.Id}";
            if (content is null) fb = $"Could not find {typeof(T).Name} with id {f.Id}";
            return (content, fb);
        };
        return BaseAction(action, f, _config.EnableDeleteAction);
    }
}