# Write a user story prompt
## Prompt
```text
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

## Result
```text
**Title:** Display Real-Time Banana Availability from External Warehouse

**As a** customer browsing the catalog page of our e-commerce website,  
**I want** to see real-time availability of bananas from the Banana Warehouse system,  
**So that** I can make informed purchasing decisions based on up-to-date inventory.

**Acceptance Criteria**

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
```


## Set the user story in the chat context prompt
```
I’m working on a feature with the following user story:
# User Story

## Title: Display Real-Time Banana Availability from External Warehouse

**As a** [add text] 
**I want** [add text],  
**So that** [add text].

## Acceptance Criteria (Rule-Oriented Format – Functional Only)
- [add text]
Save the user story to the chat context and use it as context when mentioning the phrase "Use the banana catalog user story context.".

```

# Brainstorm best practices prompt
```
Use the banana catalog user story context. 
Explain the best practices for fetching data from the asp.net core backend to the server-side rendered frontend, frontend uses ReactJs to load content asynchronously. Ensure low latency and high availability.

Use the banana catalog user story context. 
Generate functional test cases from the acceptance criteria in the banana catalog user story. Format them as Given/When/Then style, covering all scenarios: available, unavailable, unknown, partial errors, pagination, and missing banana SKUs.

Based on the banana catalog user story context, list potential edge cases we should clarify with the product owner—especially around data consistency, pagination boundaries, and error handling for partial responses from the Banana Warehouse system.

Using the banana catalog user story context, outline a recommended backend architecture for querying the Banana Warehouse system during server-side rendering. Include caching options, failure isolation strategies, and fallback behavior in case of partial or failed data fetches.
```

# Brainstorm REST API endpoints design prompt
```text
Using the banana catalog user story context, design the REST API endpoints needed to retrieve real-time banana availability from the external Banana Warehouse system.  

Include:
- Endpoint URL structure and HTTP methods  
- Request parameters (e.g., SKU list, pagination details)  
- Response schema (with all possible availability statuses: In Stock, Out of Stock, Availability Unknown)  
- How to handle errors gracefully in the API response  
- Guidelines for batching requests for 100+ bananas per page  

The goal is to enable the catalog frontend to fetch real-time availability efficiently during server-side rendering, without blocking page load.
```

# Brainstorm best practices of HTTP clients, explain details prompt
```text
As an experienced .NET developer working on a high-load microservices architecture using ASP.NET Core and .NET 8, you are tasked with integrating the Product Catalog microservice with an external Inventory System via HTTP to retrieve product availability based on SKU codes.

Describe the best practices for implementing a robust, high-performance HTTP client for communication with such external services. Include recommended resilience patterns such as Retry, Circuit Breaker, and Bulkhead Isolation. For each pattern, explain when it should be applied and why it is beneficial in high-load scenarios. Additionally, outline any other important considerations—such as timeouts, caching, connection reuse, and health checks—to ensure high availability, fault tolerance, and efficient resource usage in production-grade environments.
```

# Write HTTP client using best practices prompt
```text
As a .NET developer, implement a InventoryClient class to handle HTTP requests in a high-load system, meeting the following requirements:

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
  2. The InventoryClient implementation class.
  3. Supporting classes and utilities, if any.
3. Test Cases  
  Implement the following test methods:
  - GetInventoryDataAsync_ReturnsData_WhenExternalServiceIsAvailable
  - GetInventoryDataAsync_ReturnsCachedData_WhenCircuitBreakerIsOpen
4. Ensure the code compiles without error under .NET 8.
5. Ensure test cases pass.
6. Export Format  
  Output all C# code in one continuous copy-paste block.
he build.
7. Best Practices Commentary  
  Add inline comments explaining how optimization and reliability patterns—such as HttpClientFactory, distributed caching, circuit breaker, and bulkhead isolation—affect performance and resiliency, cache Invalidation.
```
