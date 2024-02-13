using Crudite;
using Crudite.Types;
using DividendsHelper.Core.Utils;

namespace DividendsHelper.Core.States;

public abstract class BaseState<TId, T, TRequest, TLoaded> : LoadableCrudState<TId, T, TRequest, TLoaded>
    where TId : notnull
    where T : IBaseModel<TId> {

    public BaseState(FetchingLoader<TRequest, TLoaded> loader, IDtoConverter<TRequest, TLoaded, T> dtoConverter) : base(loader, dtoConverter) { }
    protected override async Task PreLoad() {
        await base.PreLoad();
        Logger.Log($"Loading state {GetType().Name}...");
    }

    protected override async Task PostLoad() {
        await base.PostLoad();
        Logger.Log($"Loading state {GetType().Name} done.");
    }
}
