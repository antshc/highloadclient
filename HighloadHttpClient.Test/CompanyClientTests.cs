using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Wrap;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace CompanyClientExample
{
    // 1. xUnit Test Class
    public class CompanyClientTests : IDisposable
    {
        private readonly WireMockServer _wireMockServer;
        private readonly ServiceProvider _serviceProvider;
        private readonly IDistributedCache _cache;
        private readonly CompanyClient _companyClient;

        public CompanyClientTests()
        {
            // Start WireMock server to mock external API
            _wireMockServer = WireMockServer.Start();

            // Setup dependency injection
            var services = new ServiceCollection();

            // Configure in-memory distributed cache for testing
            services.AddDistributedMemoryCache();

            // Configure HttpClientFactory with Polly policies
            services.AddHttpClient("CompanyClient", client =>
            {
                client.BaseAddress = new Uri(_wireMockServer.Url);
            });

            services.AddSingleton<CompanyClient>();

            _serviceProvider = services.BuildServiceProvider();

            _cache = _serviceProvider.GetRequiredService<IDistributedCache>();
            _companyClient = new CompanyClient(
                _serviceProvider.GetRequiredService<IHttpClientFactory>(),
                _cache,
                _wireMockServer.Url);
        }

        [Fact]
        public async Task GetCompanyDataAsync_ReturnsData_WhenExternalServiceIsAvailable()
        {
            // Arrange
            var companyId = "123";
            var expectedData = new CompanyData { Id = companyId, Name = "Test Company" };

            _wireMockServer
                .Given(Request.Create().WithPath($"/companies/{companyId}").UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(expectedData)));

            // Act
            var result = await _companyClient.GetCompanyDataAsync(companyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData.Id, result.Id);
            Assert.Equal(expectedData.Name, result.Name);
        }

        [Fact]
        public async Task GetCompanyDataAsync_ReturnsCachedData_WhenCircuitBreakerIsOpen()
        {
            // Arrange
            var companyId = "456";
            var cachedData = new CompanyData { Id = companyId, Name = "Cached Company" };

            // Manually cache the data
            var cacheKey = $"CompanyData_{companyId}";
            var cachedJson = JsonSerializer.Serialize(cachedData);
            await _cache.SetStringAsync(cacheKey, cachedJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            // Configure WireMock to return 500 errors to trigger circuit breaker
            _wireMockServer
                .Given(Request.Create().WithPath($"/companies/{companyId}").UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500));

            // Trigger circuit breaker by making multiple failing requests
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await _companyClient.GetCompanyDataAsync(companyId);
                }
                catch
                {
                    // Ignore exceptions
                }
            }

            // Act
            var result = await _companyClient.GetCompanyDataAsync(companyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cachedData.Id, result.Id);
            Assert.Equal(cachedData.Name, result.Name);
        }

        public void Dispose()
        {
            _wireMockServer.Stop();
            _wireMockServer.Dispose();
            _serviceProvider.Dispose();
        }
    }

    // 2. CompanyClient Implementation Class
    public class CompanyClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        private readonly AsyncBulkheadPolicy _bulkheadPolicy;
        private readonly string _baseAddress;

        public CompanyClient(IHttpClientFactory httpClientFactory, IDistributedCache cache, string baseAddress)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _baseAddress = baseAddress;

            // Configure Circuit Breaker: open after 5 consecutive failures, stays open for 30 seconds
            _circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            // Configure Bulkhead: allow 10 concurrent executions with a queue of 20
            _bulkheadPolicy = Policy.BulkheadAsync(10, 20);
        }

        public async Task<CompanyData> GetCompanyDataAsync(string companyId)
        {
            var cacheKey = $"CompanyData_{companyId}";

            // Check if circuit breaker is open
            if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                // Attempt to retrieve data from cache
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (cachedData != null)
                {
                    return JsonSerializer.Deserialize<CompanyData>(cachedData);
                }
                else
                {
                    throw new Exception("Circuit breaker is open and no cached data is available.");
                }
            }

            try
            {
                // Execute HTTP request within bulkhead and circuit breaker policies
                var response = await _bulkheadPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(() =>
                    {
                        var client = _httpClientFactory.CreateClient("CompanyClient");
                        return client.GetAsync($"/companies/{companyId}");
                    }));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await _cache.SetStringAsync(cacheKey, content, new DistributedCacheEntryOptions
                    {
                        // If the external system remains unresponsive for a configured duration, invalidate the cached data to prevent serving stale responses.
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                    return JsonSerializer.Deserialize<CompanyData>(content);
                }
                else
                {
                    throw new Exception($"Failed to retrieve company data. Status code: {response.StatusCode}");
                }
            }
            catch (BrokenCircuitException)
            {
                // Circuit breaker is open; attempt to retrieve data from cache
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (cachedData != null)
                {
                    return JsonSerializer.Deserialize<CompanyData>(cachedData);
                }
                else
                {
                    throw new Exception("Circuit breaker is open and no cached data is available.");
                }
            }
        }
    }

    // 3. Supporting Classes and Utilities
    public class CompanyData
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
