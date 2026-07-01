# Middleware

Middleware wraps an `IAgent` and intercepts `InvokeAsync` / `InvokeStreamingAsync`. Applied via `AgentExtensions.WithMiddleware()`.

## Usage Pattern

```csharp
IAgent agent = hive.CreateAgentFrom(cfg => { ... });

// Single middleware
agent = agent.WithMiddleware(new RetryMiddleware(new RetryOptions { MaxAttempts = 3 }));

// Chained (applied inner-first)
agent = agent
    .WithMiddleware(new RetryMiddleware(new RetryOptions { MaxAttempts = 3 }))
    .WithMiddleware(new TimeoutMiddleware(new TimeoutOptions { Timeout = TimeSpan.FromSeconds(30) }))
    .WithMiddleware(new LoggingMiddleware(logger));
```

## Built-in Middleware Types

### Retry

```csharp
agent.WithMiddleware(new RetryMiddleware(new RetryOptions
{
    MaxAttempts  = 3,
    Delay        = TimeSpan.FromSeconds(1),
    BackoffType  = RetryBackoffType.Exponential,   // Linear | Exponential | Constant
    RetryOn      = ex => ex is HttpRequestException
}));
```

### Timeout

```csharp
agent.WithMiddleware(new TimeoutMiddleware(new TimeoutOptions
{
    Timeout = TimeSpan.FromSeconds(30)
}));
```

### Rate Limit

```csharp
agent.WithMiddleware(new RateLimitMiddleware(new RateLimitOptions
{
    MaxRequests = 10,
    Window      = TimeSpan.FromMinutes(1)
}));
```

### Circuit Breaker

```csharp
agent.WithMiddleware(new CircuitBreakerMiddleware(new CircuitBreakerOptions
{
    FailureThreshold = 5,
    SamplingDuration = TimeSpan.FromSeconds(30),
    BreakDuration    = TimeSpan.FromSeconds(60)
}));
```

### Bulkhead (Concurrency Limit)

```csharp
agent.WithMiddleware(new BulkheadMiddleware(new BulkheadOptions
{
    MaxConcurrency = 4,
    MaxQueue       = 10
}));
```

### Caching

```csharp
agent.WithMiddleware(new CachingMiddleware(new CachingOptions
{
    Duration   = TimeSpan.FromMinutes(5),
    KeyBuilder = messages => string.Join("|", messages.Select(m => m.GetText()))
}));
```

### Logging

```csharp
agent.WithMiddleware(new LoggingMiddleware(logger, new LoggingOptions
{
    LogRequests  = true,
    LogResponses = true,
    LogLevel     = LogLevel.Debug
}));
```

### Fallback

```csharp
agent.WithMiddleware(new FallbackMiddleware(new FallbackOptions
{
    FallbackAgent = backupAgent,
    ShouldFallback = ex => ex is not OperationCanceledException
}));
```

### Composite

```csharp
// Group multiple middleware as a single unit
var composite = new CompositeMiddleware(
    new RetryMiddleware(...),
    new TimeoutMiddleware(...)
);
agent.WithMiddleware(composite);
```

## Custom Middleware

```csharp
public class MyMiddleware : IAgentMiddleware
{
    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentMiddlewareDelegate next,
        CancellationToken ct = default)
    {
        // pre-processing
        Console.WriteLine("Before invoke");

        var response = await next(agent, messages, ct);

        // post-processing
        Console.WriteLine("After invoke");
        return response;
    }
}
```
