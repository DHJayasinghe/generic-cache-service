# Memory Cache Service

The Memory Cache Service is a C# library designed to simplify caching of data retrieved from external sources or expensive operations. It provides thread-safe access to cached data with optional expiration times, ensuring efficient and responsive application performance.

## Features
- **Thread-safe caching**: Safely retrieves and stores data in memory, handling concurrent access using SemaphoreSlim.
- **Delegates**: Allows passing a factory method to update the cache, enabling dynamic data retrieval and storage.
- **One concurrent caller**: Ensures that only one concurrent caller for a given key executes the factory method, preventing cache stampede and redundant data fetching.
- **Customizable expiration**: Supports both absolute expiration and sliding expiration policies to manage cache freshness and memory usage efficiently.
- **Efficient cache management**: : Utilizes IMemoryCache from .NET Core to efficiently manage cached items, optimizing memory usage and improving application performance.

## Usage

1. **Initialize Memory Cache Service**
```
// Initialize MemoryCacheService using Dependency Injection
services.AddMemoryCache();
services.AddSingleton<MemoryCacheService>()
```
2. **Cache Data Retrieval**
Use TryGetValueAsync method to retrieve data from cache or fetch it if not cached:
```
// get cache or update if not cached
var cacheResult = await memoryCacheService.TryGetValueAsync(
    "cache_key",
    () => FetchDataFromExternalSourceAsync(), // Delegate to fetch data if not cached
    TimeSpan.FromMinutes(5)
);

// get cache without updating
var cacheResult = await memoryCacheService.TryGetValueAsync("cache_key");

// Access cache result
var cachedData = cacheResult.Value;
```

3. **Customize Cache Expiration**
You can customize expiration policies using absoluteExpiry and slidingExpiry parameters in TryGetValueAsync method
```
var cacheResult = await memoryCacheService.TryGetValueAsync(
    "cache_key",
    () => FetchDataFromExternalSourceAsync(),
    absoluteExpiry: TimeSpan.FromMinutes(30), // Absolute expiration time
    slidingExpiry: TimeSpan.FromMinutes(5)    // Sliding expiration time
);
```

## Test Result of Concurrent Requests Accessing the Cache
The following results demonstrate the effectiveness of the Memory Cache Service in handling concurrent requests and ensuring efficient data retrieval from external sources.
- **Hits**: Number of times data was retrieved from the cache.
- **Misses**: Number of times data was fetched from the external source due to cache miss.
- **Total**: Requests: Total number of requests made for each key.

![image](https://github.com/DHJayasinghe/generic-cache-service/assets/26274468/c7714805-3ef3-4b20-a3e3-1c71fa570845)
