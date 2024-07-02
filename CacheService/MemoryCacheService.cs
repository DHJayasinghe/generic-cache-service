using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace CacheService;

public class MemoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<CacheResult<T>> GetOrCreate<T>(string key, Func<ICacheEntry, Task<T>> createItem)
    {
        if (_cache.TryGetValue(key, out T cacheEntry))
        {
            return new CacheResult<T>(cacheEntry, true);
        }

        var lockObj = _locks.GetOrAdd(key, new SemaphoreSlim(1, 1));

        await lockObj.WaitAsync();

        try
        {
            if (_cache.TryGetValue(key, out cacheEntry))
            {
                return new CacheResult<T>(cacheEntry, true);
            }

            cacheEntry = await _cache.GetOrCreateAsync(key, createItem);
            return new CacheResult<T>(cacheEntry, false);
        }
        finally
        {
            lockObj.Release();
            _locks.TryRemove(key, out _);
        }
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        return _cache.TryGetValue(key, out value);
    }
}

public class CacheResult<T>
{
    public T Value { get; }
    public bool IsCacheHit { get; }

    public CacheResult(T value, bool isCacheHit)
    {
        Value = value;
        IsCacheHit = isCacheHit;
    }
}
