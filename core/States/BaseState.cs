using Beef.Fetchers;
using DividendsHelper.Core.Utils;
using DividendsHelper.Models.Core;

namespace DividendsHelper.Core.States;

public interface IBaseState<TId, T>
    where TId : notnull
    where T : IBaseModel<TId> {
    IEnumerable<T> Create(IEnumerable<T> dtos);
    Task<T?> Read(TId id);
    IEnumerable<T> Update(IEnumerable<T> dtos);
    Task<T?> Delete(TId id);
}

public abstract class BaseState<TId, T, TRequest, TDto> : IBaseState<TId, T>
    where TId : notnull
    where T : class, IBaseModel<TId> {

    private readonly Dictionary<TId, T> _cache = new();

    protected readonly object Locker = new();
    
    protected abstract IB3Fetcher<TRequest, TDto> GetFetcher();

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
        var ret = Create(request, res);
        return ret.Count();
    }

    public async Task<int> Fetch(TRequest request) {
        Logger.Log($"Fetching {typeof(T).Name} data for {request}...");
        var count = await FetchAndInsert(request);
        Logger.Log($"Fetching {typeof(T).Name} data for {request} done. Fetched {count} items.");
        return count;
    }

    protected virtual T Create(T value) {
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
    public T Create(TRequest req, TDto dto) => Create(ConvertDto(req, dto));
    public virtual IEnumerable<T> Create(TRequest req, IEnumerable<TDto> dtos) {
        var values = dtos.Select(dto => ConvertDto(req, dto));
        return Create(values);
    }
    public IEnumerable<T> Create(IEnumerable<T> values) {
        var ret = new List<T>();
        foreach (var v in values)
            ret.Add(Create(v));
        return ret;
    }

    public virtual Task<T?> Read(TId id)
    {
        T? value;
        lock (Locker) {
            if (!_cache.TryGetValue(id, out value))
                return Task.FromResult<T?>(null);
        }
        return Task.FromResult<T?>(value);
    }

    protected virtual T? Update(T value) {
        T? current;
        lock (Locker) {
            if (!_cache.TryGetValue(value.Id, out current)) {
                Console.WriteLine($"{GetType().Name}: Could not find Id {value.Id} on update");
                return null;
            }
            _cache[value.Id] = value;
        }
        return value;
    }

    public IEnumerable<T> Update(IEnumerable<T> dtos) {
        var ret = new List<T>();
        foreach (var dto in dtos) {
            var updated = Update(dto);
            if (updated is not null)
                ret.Add(updated);
        }
        return ret;
    }

    public async Task<T?> Delete(TId id) {
        var current = await Read(id);
        if (current is null) return null;
        lock (Locker) {
            _cache.Remove(id);
        }
        return current;
    }
}

