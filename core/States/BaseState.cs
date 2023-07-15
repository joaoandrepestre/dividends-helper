using DividendsHelper.Core.Fetching;
using DividendsHelper.Core.Utils;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States;

public interface IBaseState<TId, T>
    where TId : notnull
    where T : IBaseModel<TId> {
    Task<T?> Get(TId id);
}

public abstract class BaseState<TId, T, TRequest, TDto> : IBaseState<TId, T>
    where TId : notnull
    where T : class, IBaseModel<TId> {

    private readonly Dictionary<TId, T> _cache = new();

    protected readonly object Locker = new();
    
    protected abstract IBaseFetcher<TRequest, TDto> GetFetcher();

    public async Task Load(IEnumerable<TRequest> loadRequests) {
        Logger.Log($"Loading state {GetType().Name}...");
        Logger.Log("Initial fetch...");
        await Task.WhenAll(loadRequests.Select(Fetch));
        Logger.Log("Initial fetch done.");
        Logger.Log($"Loading state {GetType().Name} done.");
    }

    protected virtual async Task<int> FetchAndInsert(TRequest request) {
        var res = await GetFetcher().Fetch(request);
        if (res is null || !res.Any()) return 0;
        var ret = Insert(request, res);
        return ret.Count();
    }

    public async Task<int> Fetch(TRequest request) {
        Logger.Log($"Fetching {typeof(T).Name} data for {request}...");
        var count = await FetchAndInsert(request);
        Logger.Log($"Fetching {typeof(T).Name} data for {request} done. Fetched {count} items.");
        return count;
    }

    protected virtual T Insert(T value) {
        T? current;
        lock (Locker) {
            if (_cache.TryGetValue(value.Id, out current)) {
                return current;
            }
            _cache.Add(value.Id, value);
        }
        if (current != default(T))
            Console.WriteLine($"{GetType().Name}: Duplicate Id {value.Id}");
        return value;
    }

    protected abstract T ConvertDto(TRequest req, TDto dto);
    public T Insert(TRequest req, TDto dto) => Insert(ConvertDto(req, dto));
    public virtual IEnumerable<T> Insert(TRequest req, IEnumerable<TDto> dtos) {
        var values = dtos.Select(dto => ConvertDto(req, dto));
        return Insert(values);
    }
    public IEnumerable<T> Insert(IEnumerable<T> values) {
        var ret = new List<T>();
        foreach (var v in values)
            ret.Add(Insert(v));
        return ret;
    }

    public virtual Task<T?> Get(TId id)
    {
        T? value;
        lock (Locker) {
            if (!_cache.TryGetValue(id, out value))
                return Task.FromResult<T?>(null);
        }
        return Task.FromResult<T?>(value);
    }
}

