using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace CacheService;

public class MemoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public async Task<CacheResult<T>> TryGetValueAsync<T>(string key, Func<Task<T>> createItem = null, TimeSpan? absoluteExpiry = null, TimeSpan? slidingExpiry = null)
    {
        if (_cache.TryGetValue(key, out T existingEntry))
            return new CacheResult<T>(existingEntry, true);

        var lockObj = _locks.GetOrAdd(key, new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync();

        try
        {
            if (_cache.TryGetValue(key, out T cacheEntry))
                return new CacheResult<T>(cacheEntry, true);

            if (createItem == null) return default;

            T result = await createItem();
            if (result is not null) // cache positive responses only
            {
                _cache.Set(key, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiry,
                    SlidingExpiration = slidingExpiry
                });
            }
            return new CacheResult<T>(result, false);
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
