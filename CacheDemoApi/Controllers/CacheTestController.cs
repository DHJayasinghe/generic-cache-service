using CacheService;
using Microsoft.AspNetCore.Mvc;

namespace CacheServiceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheTestController(
        MemoryCacheService memoryCache,
        IHttpClientFactory httpClientFactory
    ) : ControllerBase
    {
        private readonly MemoryCacheService _memoryCache = memoryCache;

        [HttpGet]
        public async Task<Response> Get(string key)
        {
            var cacheValue = await _memoryCache.TryGetValueAsync(key, () => GetSlowApiDataAsync(), TimeSpan.FromMinutes(5));
            if (cacheValue.Value is null) throw new Exception("Failed to fetch data from the API");

            return new Response(cacheValue.IsCacheHit, cacheValue.Value);
        }

        private async Task<IEnumerable<TodoItem>> GetSlowApiDataAsync()
        {
            Console.WriteLine("Fetching from data source");

            // Simulate a slow response by waiting for 3 seconds
            await Task.Delay(3000);

            using var httpClient = httpClientFactory.CreateClient();
            return await httpClient.GetFromJsonAsync<IEnumerable<TodoItem>>("https://jsonplaceholder.typicode.com/posts");
        }
    }

    public record TodoItem(int UserId, int Id, string Title, bool Completed);
    public record Response(bool CacheHit, IEnumerable<TodoItem> Data);
}