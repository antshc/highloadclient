# 📦 Real-Time Banana Availability Integration – Backend to React SSR (Best Practices)

_Using the **banana catalog user story context**_

## ✅ Architecture Overview

```
ReactJS SSR Page
   ↓ (Initial SSR + hydration)
ASP.NET Core Backend (Catalog API)
   ↓
Banana Warehouse Service (HTTP integration)
```

---

## 🔹 Best Practices for Server-Side Data Fetching

### 1. **Parallel Batched Fetching by SKU**
- On page load, collect all visible banana SKUs (min 100).
- Query Banana Warehouse in a single batched HTTP call:
  ```
  GET /availability?skus=banana1,banana2,...banana100
  ```

### 2. **Non-Blocking SSR with Deferred Data**
- Render catalog page structure first (placeholders if needed).
- Defer availability fetching until client hydration (React `useEffect` or React Query).
- Improves SSR performance and resilience.

### 3. **Async Availability API Endpoint**
- Expose:
  ```
  GET /api/availability?skus=banana1,banana2,...
  ```
- Fetch this data client-side post-hydration:
  ```ts
  useEffect(() => {
    fetch(`/api/availability?skus=${skuList.join(',')}`)
      .then(res => res.json())
      .then(setAvailability);
  }, []);
  ```

---

## 🔹 ASP.NET Core Backend Best Practices

### 4. **HttpClientFactory + Resilience**
Use `IHttpClientFactory` with **Polly** for resilience:
- **Retry**: Handle transient HTTP errors (e.g., 500, timeout).
- **Circuit Breaker**: Stop repeated calls when Banana Warehouse is down.
- **Timeouts**: Enforce short limits (1–2s).
- **Bulkhead**: Isolate failure & avoid thread exhaustion.

### 5. **Graceful Degradation**
- If availability fails:
  - Return `"Availability Unknown"`.
  - Allow page to render without blocking.

### 6. **Short-Term Caching**
- Cache availability for 30–60s in Redis (or in-memory for dev).
- Reduces pressure under burst loads.

```csharp
var cacheKey = $"availability:{sku}";
var cached = await _cache.GetStringAsync(cacheKey);
```

---

## 🔹 Frontend (ReactJS) Resilience

### 7. **Render Fallbacks**
- Show placeholders: “Checking availability…”
- If timeout or error: show “Availability Unknown”.

### 8. **Pagination-Aware Fetching**
- Each catalog page fetches only visible banana SKUs.
- Prevent stale/overlapping state across paginated views.

---

## 🔹 Monitoring & Observability

### 9. **Metrics**
Track:
- API response times
- Circuit breaker states
- SSR Time to First Byte (TTFB)
- Error counts per SKU

### 10. **Logging & Tracing**
- Add correlation IDs to catalog + availability flow.
- Log degraded or fallback states for diagnosis.

---

## 🧩 Summary

| Concern                 | Best Practice                                                  |
|------------------------|----------------------------------------------------------------|
| Data Fetching          | Batched by SKU, async API, fetch from React after SSR         |
| Resilience             | Retry + Circuit Breaker + Timeout + Bulkhead via Polly        |
| Performance            | Parallel HTTP calls, cache short-lived results                |
| Frontend Integration   | Show placeholders, fallback gracefully in UI                  |
| Availability Failures  | Do not block page render, show “Availability Unknown”         |
| Pagination             | Isolate fetch to visible SKUs only per page                   |

---
