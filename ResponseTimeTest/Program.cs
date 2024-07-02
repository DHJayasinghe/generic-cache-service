using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;


var httpClient = new HttpClient();
ConcurrentDictionary<string, (int Hits, int Misses)> CacheSummary = new();

var tasks = Enumerable.Range(1, 500).Select(_ => MakeRequest(httpClient, "abc"));
await Task.WhenAll(tasks);

Console.WriteLine("Test run completed.");
PrintSummary();

async Task MakeRequest(HttpClient httpClient, string key)
{
    var stopwatch = Stopwatch.StartNew();

    var response = await httpClient.GetAsync($"http://localhost:5179/cachetest?key={key}");
    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadAsStringAsync();
    var result = JsonConvert.DeserializeObject<Response>(responseBody);

    stopwatch.Stop();

    Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds} ms, Cache Hit: {result.CacheHit}");

    UpdateSummary(key, result.CacheHit);
}

void UpdateSummary(string key, bool cacheHit)
{
    CacheSummary.AddOrUpdate(key,
        _ => cacheHit ? (Hits: 1, Misses: 0) : (Hits: 0, Misses: 1),
        (_, current) => cacheHit ? (Hits: current.Hits + 1, Misses: current.Misses) : (Hits: current.Hits, Misses: current.Misses + 1));
}

void PrintSummary()
{
    Console.WriteLine("Test Result Summary:");
    foreach (var entry in CacheSummary)
    {
        Console.WriteLine($"Key: {entry.Key}, Hits: {entry.Value.Hits}, Misses: {entry.Value.Misses}, Total Requests: {entry.Value.Hits + entry.Value.Misses}");
    }
}

public record Response(bool CacheHit);