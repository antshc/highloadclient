## ChatGPT 4o Prompt


# User Story

## Promt for user story
```
Write a user story with acceptance criteria for the following task:

Task: 
We’re launching a new real-time inventory integration with our external Banana Warehouse system. 
We need to show availability for 100+ bananas per page, server-side rendered. 
Also, make it fast. And reliable.

Requirements:
- Use the Rule-Oriented Format for acceptance criteria: one bullet point per rule.
- Acceptance criteria must include only functional requirements—focus on behaviors, outputs, and system responses to user or system actions.
- Exclude all non-functional requirements (e.g., response time limits, system uptime, latency, scalability, performance thresholds).
- Ensure all criteria are testable from a functional standpoint.
```

## **Title:** Display Real-Time Banana Availability from External Warehouse

**As a** customer browsing the catalog page of our e-commerce website,  
**I want** to see real-time availability of bananas from the Banana Warehouse system,  
**So that** I can make informed purchasing decisions based on up-to-date inventory.

---

## Acceptance Criteria (Rule-Oriented Format – Functional Only)

- The system must display the availability status for each banana product retrieved from the Banana Warehouse system.
- For each catalog page load, the system must query availability data for all bananas visible on that page (minimum of 100 items).
- If a banana is available in the Banana Warehouse system, the catalog must display the label “In Stock”.
- If a banana is not available in the Banana Warehouse system, the catalog must display the label “Out of Stock”.
- If the Banana Warehouse system returns an unknown or missing availability status for a banana, the catalog must display the label “Availability Unknown”.
- The system must not block or prevent the rendering of the catalog page if the availability data from the Banana Warehouse system is delayed or unavailable.
- The system must allow users to navigate between paginated catalog views, each showing the correct availability status for all listed bananas.
- If a user reloads the catalog page, the system must re-fetch availability data for the bananas on that page from the Banana Warehouse system.
- If the Banana Warehouse system returns an error for one or more bananas, the catalog must still render the page and show “Availability Unknown” for the affected items only.
- When a banana is no longer listed in the Banana Warehouse system, the availability status must not be shown for that item on the catalog page.


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
