## ChatGPT 4o Prompt

```text
As an experienced .NET developer working on a high-load microservices architecture using ASP.NET Core and .NET 8, you are tasked with integrating the Product Catalog microservice with an external Inventory System via HTTP to retrieve product availability based on SKU codes.

Describe the best practices for implementing a robust, high-performance HTTP client for communication with such external services. Include recommended resilience patterns such as Retry, Circuit Breaker, and Bulkhead Isolation. For each pattern, explain when it should be applied and why it is beneficial in high-load scenarios. Additionally, outline any other important considerations—such as timeouts, caching, connection reuse, and health checks—to ensure high availability, fault tolerance, and efficient resource usage in production-grade environments.
```

```text
As a .NET developer, implement a CompanyClient class to handle HTTP requests in a high-load system, meeting the following requirements:

Requirements:
1. HttpClientFactory Integration
   - Use IHttpClientFactory to create an HttpClient instance for communicating with an external system.
2. Distributed Caching
   - Integrate a distributed cache (e.g., via IDistributedCache) to store and retrieve data efficiently, and use the InMemory option for testing purposes.
3. Circuit Breaker Pattern
   - Apply the Circuit Breaker pattern using Polly to monitor external system availability.
   - When the external system becomes unresponsive, the circuit should open to prevent further calls.
4. Fallback to Cache
   - When the circuit breaker is open (indicating external system downtime), the client should:
     - Serve responses from the distributed cache.
     - Refrain from making calls to the external system until it becomes healthy again.
5. Cache Invalidation
   - If the external system remains unresponsive for a configured duration, invalidate the cached data to prevent serving stale responses.
6. Bulkhead Isolation
   - Implement the Bulkhead pattern using Polly to limit the number of concurrent requests, thereby preventing thread exhaustion and isolating failures.
7. Use WireMock .Net to mock the external system

Generate a complete, buildable C# .NET 8 code file implementing the following:
1. NuGet Package References  
  Provide a list of required NuGet packages under a single <ItemGroup> using their latest stable versions. Place this list in a separate copy-paste block.
2. Test Structure  
  Output a single .cs file that includes:
  1. An Xunit test class at the top.
  2. The CompanyClient implementation class.
  3. Supporting classes and utilities, if any.
3. Test Cases  
  Implement the following test methods:
  - GetCompanyDataAsync_ReturnsData_WhenExternalServiceIsAvailable
  - GetCompanyDataAsync_ReturnsCachedData_WhenCircuitBreakerIsOpen
4. Ensure the code compiles without error under .NET 8.
5. Export Format  
  Output all C# code in one continuous copy-paste block.
he build.
6. Best Practices Commentary  
  Add inline comments explaining how optimization and reliability patterns—such as HttpClientFactory, distributed caching, circuit breaker, and bulkhead isolation—affect performance and resiliency, cache Invalidation.
```
