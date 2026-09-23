# Middleware

Middleware wraps an `IAgent` and intercepts `InvokeAsync` / `InvokeStreamingAsync`. Applied via `AgentExtensions.WithMiddleware()`.

> **Note**: This is `IAgentMiddleware` — agent-level (one whole turn). For middleware around
> each generator call inside `MessageService`'s tool loop, see `IMessageMiddleware` in
> [SERVICES.md](SERVICES.md#imessagemiddleware) — a separate concept at a lower layer.

## Usage Pattern

```csharp
IAgent agent = hive.CreateAgentFrom(cfg => { ... });

// Single middleware
agent = agent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// Several at once — executed left to right (the first one is outermost)
agent = agent.WithMiddleware(
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(new RetryMiddlewareOptions { MaxRetries = 3 }),
    new TimeoutMiddleware(new TimeoutMiddlewareOptions { Timeout = TimeSpan.FromSeconds(30) }));
```

Every built-in middleware takes a `<Name>MiddlewareOptions` object; most also have a shorthand
constructor (`RetryMiddleware(int maxRetries)`, `TimeoutMiddleware(TimeSpan)`, `RateLimitMiddleware(int, TimeSpan)`,
`CircuitBreakerMiddleware(int, TimeSpan)`, `BulkheadMiddleware(int[, int])`, `CachingMiddleware(TimeSpan)`,
`LoggingMiddleware(Action<string>)`, `FallbackMiddleware(IAgent)`).

## Built-in Middleware Types

### Retry

```csharp
agent.WithMiddleware(new RetryMiddleware(new RetryMiddlewareOptions
{
    MaxRetries        = 3,
    InitialDelay      = TimeSpan.FromSeconds(1),
    MaxDelay          = TimeSpan.FromSeconds(30),
    BackoffMultiplier = 2.0,                              // exponential backoff
    JitterFactor      = 0.2,
    ShouldRetry       = ex => ex is HttpRequestException
}));
```

### Timeout

```csharp
agent.WithMiddleware(new TimeoutMiddleware(new TimeoutMiddlewareOptions
{
    Timeout = TimeSpan.FromSeconds(30)
}));
```

### Rate Limit

```csharp
agent.WithMiddleware(new RateLimitMiddleware(new RateLimitMiddlewareOptions
{
    MaxRequests = 10,
    Window      = TimeSpan.FromMinutes(1)
}));
```

### Circuit Breaker

```csharp
agent.WithMiddleware(new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
{
    FailureThreshold = 5,
    FailureWindow    = TimeSpan.FromSeconds(30),
    BreakDuration    = TimeSpan.FromSeconds(60)
}));
```

### Bulkhead (Concurrency Limit)

```csharp
agent.WithMiddleware(new BulkheadMiddleware(new BulkheadMiddlewareOptions
{
    MaxConcurrency = 4,
    MaxQueueSize   = 10
}));
```

### Caching

```csharp
agent.WithMiddleware(new CachingMiddleware(new CachingMiddlewareOptions
{
    Expiration               = TimeSpan.FromMinutes(5),
    MaxCacheSize             = 1000,
    IncludeInstructionsInKey = true    // the key is computed from the messages (and options); there is no custom key builder
}));
```

### Logging

```csharp
agent.WithMiddleware(new LoggingMiddleware(new LoggingMiddlewareOptions
{
    LogAction              = Console.WriteLine,        // any Action<string> sink (default: none)
    IncludeMessagePreview  = true,
    IncludeResponsePreview = true,
    MaxPreviewLength       = 100
}));
```

### Fallback

```csharp
agent.WithMiddleware(new FallbackMiddleware(new FallbackMiddlewareOptions
{
    FallbackAgent  = backupAgent,
    ShouldFallback = ex => ex is not OperationCanceledException
}));
```

### Composite

```csharp
// Group multiple middleware as a single named unit
var composite = new CompositeMiddleware("resilience",
    new RetryMiddleware(maxRetries: 3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30)));
agent.WithMiddleware(composite);

// Or use a ready-made pack
agent.WithMiddleware(MiddlewarePacks.Resilience(maxRetries: 3, timeout: TimeSpan.FromSeconds(30)));
```

## Custom Middleware

```csharp
public class MyMiddleware : IAgentMiddleware
{
    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // pre-processing
        Console.WriteLine("Before invoke");

        var response = await next(messages, options);

        // post-processing
        Console.WriteLine("After invoke");
        return response;
    }
}
```

Middleware must forward the per-request `AgentInvokeOptions` to `next`. Middleware that
affects responses (e.g. caching) must account for options — `CachingMiddleware` includes
options in its cache key, so identical messages with different options never share an entry.
