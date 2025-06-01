using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

public class InventoryClientTests
{
    private readonly InventoryClient _client;
    private readonly WireMockServer _mockServer;

    public InventoryClientTests()
    {
        _mockServer = WireMockServer.Start();

        // Setup a working endpoint that returns a JSON object
        _mockServer.Given(Request.Create().WithPath("/inventory").UsingGet())
                   .RespondWith(Response.Create()
                                      .WithStatusCode(200)
                                      .WithBodyAsJson(new { sku = "banana-001", available = true }));

        var services = new ServiceCollection();
        services.AddHttpClient("InventoryClient", client =>
        {
            client.BaseAddress = new Uri(_mockServer.Url);
            client.Timeout = TimeSpan.FromSeconds(2);
        });

        services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
        services.AddSingleton<InventoryClient>();

        var provider = services.BuildServiceProvider();
        _client = provider.GetRequiredService<InventoryClient>();
    }

    [Fact]
    public async Task GetInventoryDataAsync_ReturnsData_WhenExternalServiceIsAvailable()
    {
        string sku = "banana-001";
        string data = await _client.GetInventoryDataAsync(sku);
        Assert.Contains("banana-001", data);
    }

    [Fact]
    public async Task GetInventoryDataAsync_ReturnsCachedData_WhenCircuitBreakerIsOpen()
    {
        string sku = "banana-001";

        // Step 1: Ensure successful call and cache population
        _mockServer.ResetMappings();
        _mockServer.Given(Request.Create().WithPath("/inventory").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(200)
                                          .WithBodyAsJson(new { sku = sku, available = true }));

        var original = await _client.GetInventoryDataAsync(sku); // cache this value

        // Step 2: Trip the circuit breaker by returning errors
        _mockServer.ResetMappings();
        _mockServer.Given(Request.Create().WithPath("/inventory").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(500));

        try { await _client.GetInventoryDataAsync(sku); } catch { }
        try { await _client.GetInventoryDataAsync(sku); } catch { }

        // Step 3: Now the circuit is open, it should fallback to cache
        string cached = await _client.GetInventoryDataAsync(sku);
        Assert.Equal(original, cached); // ✅ fallback worked
    }
}

public class InventoryClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache _cache;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;
    private readonly AsyncBulkheadPolicy _bulkhead;
    private const string CachePrefix = "inventory_";
    // ✅ Cache data for 5 minutes, stale after 10 minutes
    private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(10);

    public InventoryClient(IHttpClientFactory httpClientFactory, IDistributedCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;

        // ✅ Circuit Breaker: Prevent hammering failed services
        _circuitBreaker = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        // ✅ Bulkhead: Limit concurrent requests to protect resources
        _bulkhead = Policy.BulkheadAsync(maxParallelization: 5, maxQueuingActions: 10);
    }

    public async Task<string> GetInventoryDataAsync(string sku)
    {
        string cacheKey = $"{CachePrefix}{sku}";
        string? cached = await _cache.GetStringAsync(cacheKey);

        if (_circuitBreaker.CircuitState == CircuitState.Open)
        {
            // ✅ Fallback: return cached data if circuit is open
            if (cached is not null)
                return cached;
            throw new Exception("External service is down and no cache is available.");
        }

        return await _bulkhead.ExecuteAsync(async () =>
        {
            try
            {
                var client = _httpClientFactory.CreateClient("InventoryClient");
                var response = await _circuitBreaker.ExecuteAsync(() => client.GetAsync($"/inventory?sku={sku}"));
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // ✅ Save response to distributed cache
                await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheTTL
                });

                return json;
            }
            catch
            {
                // ✅ Fallback if cache is still valid
                if (cached is not null)
                    return cached;

                throw;
            }
        });
    }
}
