using CacheService;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services
    .AddControllers();
services
    .AddMemoryCache();
services
    .AddSingleton<MemoryCacheService>()
    .AddHttpClient();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
