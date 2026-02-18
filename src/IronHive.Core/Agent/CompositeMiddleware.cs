using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 여러 미들웨어를 하나로 조합하여 재사용 가능한 미들웨어 팩을 생성합니다.
/// </summary>
public class CompositeMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
{
    private readonly List<IAgentMiddleware> _middlewares;
    private readonly string _name;

    public CompositeMiddleware(string name, params IAgentMiddleware[] middlewares)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _middlewares = middlewares?.ToList() ?? throw new ArgumentNullException(nameof(middlewares));

        if (_middlewares.Count == 0)
        {
            throw new ArgumentException("At least one middleware is required.", nameof(middlewares));
        }
    }

    public CompositeMiddleware(string name, IEnumerable<IAgentMiddleware> middlewares)
        : this(name, middlewares?.ToArray() ?? throw new ArgumentNullException(nameof(middlewares)))
    {
    }

    /// <summary>
    /// 이 CompositeMiddleware의 이름
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 포함된 미들웨어 수
    /// </summary>
    public int Count => _middlewares.Count;

    /// <summary>
    /// 포함된 미들웨어 목록
    /// </summary>
    public IReadOnlyList<IAgentMiddleware> Middlewares => _middlewares;

    public Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // 미들웨어 체인 구성 (역순으로 감싸기)
        var pipeline = next;

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentPipeline = pipeline;
            pipeline = msgs => middleware.InvokeAsync(agent, msgs, currentPipeline, cancellationToken);
        }

        return pipeline(messages);
    }

    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // 스트리밍 지원 미들웨어만 필터링
        var streamingMiddlewares = _middlewares
            .OfType<IStreamingAgentMiddleware>()
            .ToList();

        if (streamingMiddlewares.Count == 0)
        {
            // 스트리밍 미들웨어가 없으면 바로 next 호출
            return next(messages);
        }

        // 스트리밍 미들웨어 체인 구성
        var pipeline = next;

        for (int i = streamingMiddlewares.Count - 1; i >= 0; i--)
        {
            var middleware = streamingMiddlewares[i];
            var currentPipeline = pipeline;
            pipeline = msgs => middleware.InvokeStreamingAsync(agent, msgs, currentPipeline, cancellationToken);
        }

        return pipeline(messages);
    }

    /// <summary>
    /// 새 미들웨어를 추가한 새로운 CompositeMiddleware를 반환합니다.
    /// </summary>
    public CompositeMiddleware With(IAgentMiddleware middleware)
    {
        var newList = _middlewares.ToList();
        newList.Add(middleware);
        return new CompositeMiddleware(_name, newList);
    }

    /// <summary>
    /// 새 미들웨어를 앞에 추가한 새로운 CompositeMiddleware를 반환합니다.
    /// </summary>
    public CompositeMiddleware Prepend(IAgentMiddleware middleware)
    {
        var newList = new List<IAgentMiddleware> { middleware };
        newList.AddRange(_middlewares);
        return new CompositeMiddleware(_name, newList);
    }
}

/// <summary>
/// 자주 사용되는 미들웨어 조합을 미리 정의한 팩토리
/// </summary>
public static class MiddlewarePacks
{
    /// <summary>
    /// 기본 복원력 팩: 재시도 + 타임아웃
    /// </summary>
    public static CompositeMiddleware Resilience(
        int maxRetries = 3,
        TimeSpan? timeout = null,
        TimeSpan? retryDelay = null)
    {
        return new CompositeMiddleware("resilience",
            new RetryMiddleware(new RetryMiddlewareOptions
            {
                MaxRetries = maxRetries,
                InitialDelay = retryDelay ?? TimeSpan.FromMilliseconds(500),
                BackoffMultiplier = 2.0
            }),
            new TimeoutMiddleware(timeout ?? TimeSpan.FromSeconds(30)));
    }

    /// <summary>
    /// 고급 복원력 팩: 재시도 + 타임아웃 + 회로 차단기
    /// </summary>
    public static CompositeMiddleware AdvancedResilience(
        int maxRetries = 3,
        TimeSpan? timeout = null,
        int circuitBreakerThreshold = 5,
        TimeSpan? breakDuration = null)
    {
        return new CompositeMiddleware("advanced-resilience",
            new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
            {
                FailureThreshold = circuitBreakerThreshold,
                BreakDuration = breakDuration ?? TimeSpan.FromSeconds(30)
            }),
            new RetryMiddleware(new RetryMiddlewareOptions
            {
                MaxRetries = maxRetries,
                InitialDelay = TimeSpan.FromMilliseconds(500),
                BackoffMultiplier = 2.0
            }),
            new TimeoutMiddleware(timeout ?? TimeSpan.FromSeconds(30)));
    }

    /// <summary>
    /// 관측성 팩: 로깅
    /// </summary>
    public static CompositeMiddleware Observability(Action<string>? logAction = null)
    {
        return new CompositeMiddleware("observability",
            new LoggingMiddleware(new LoggingMiddlewareOptions
            {
                LogAction = logAction
            }));
    }

    /// <summary>
    /// 리소스 보호 팩: Rate Limit + Bulkhead
    /// </summary>
    public static CompositeMiddleware ResourceProtection(
        int maxConcurrency = 10,
        int maxRequestsPerMinute = 60)
    {
        return new CompositeMiddleware("resource-protection",
            new BulkheadMiddleware(maxConcurrency),
            new RateLimitMiddleware(maxRequestsPerMinute, TimeSpan.FromMinutes(1)));
    }

    /// <summary>
    /// 프로덕션 팩: 로깅 + 회로 차단기 + 재시도 + 타임아웃 + Rate Limit
    /// </summary>
    public static CompositeMiddleware Production(
        int maxRetries = 3,
        TimeSpan? timeout = null,
        int circuitBreakerThreshold = 5,
        int maxRequestsPerMinute = 60,
        Action<string>? logAction = null)
    {
        return new CompositeMiddleware("production",
            new LoggingMiddleware(new LoggingMiddlewareOptions
            {
                LogAction = logAction
            }),
            new RateLimitMiddleware(maxRequestsPerMinute, TimeSpan.FromMinutes(1)),
            new CircuitBreakerMiddleware(new CircuitBreakerMiddlewareOptions
            {
                FailureThreshold = circuitBreakerThreshold,
                BreakDuration = TimeSpan.FromSeconds(30)
            }),
            new RetryMiddleware(new RetryMiddlewareOptions
            {
                MaxRetries = maxRetries,
                InitialDelay = TimeSpan.FromMilliseconds(500),
                BackoffMultiplier = 2.0
            }),
            new TimeoutMiddleware(timeout ?? TimeSpan.FromSeconds(30)));
    }
}
