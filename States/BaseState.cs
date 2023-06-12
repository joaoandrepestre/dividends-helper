using DividendsHelper.Fetching;
using DividendsHelper.Models;

namespace DividendsHelper.States;

public abstract class BaseState<TId, T, TRequest, TDto>
    where TId : notnull
    where T : class, IBaseModel<TId> {

    private readonly Dictionary<TId, T> cache = new();

    protected object _locker = new();

    protected abstract IBaseFetcher<TRequest, TDto> GetFetcher();

    public async Task Load(IEnumerable<TRequest> loadRequests) {
        Console.WriteLine($"Loading state {GetType().Name}...");
        Console.WriteLine("Initial fetch...");
        foreach (var request in loadRequests) {
            Console.WriteLine($"Fetching {typeof(T).Name} data for {request}...");
            await Fetch(request);
        }
        Console.WriteLine("Initial fetch done.");
        Console.WriteLine($"Loading state {GetType().Name} done.");
    }

    public async Task<IEnumerable<T>> Fetch(TRequest request) {
        var res = await GetFetcher().Fetch(request);
        if (res is null || !res.Any()) return Enumerable.Empty<T>(); ;
        return Insert(request, res);
    }

    public virtual T Insert(T value) {
        var current = value;
        lock (_locker) {
            if (cache.TryGetValue(value.Id, out current)) {
                return current;
            }
            cache.Add(value.Id, value);
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
    public virtual IEnumerable<T> Insert(IEnumerable<T> values) {
        var ret = new List<T>();
        foreach (var v in values)
            ret.Add(Insert(v));
        return ret;
    }

    public virtual T? Get(TId id) {
        if (!cache.TryGetValue(id, out var value))
            return null;
        return value;
    }
}

