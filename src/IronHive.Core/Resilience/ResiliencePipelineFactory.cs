using System.Net;
using IronHive.Abstractions.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace IronHive.Core.Resilience;

/// <summary>
/// AI 작업을 위한 Resilience Pipeline을 생성하는 팩토리입니다.
/// </summary>
public static class ResiliencePipelineFactory
{
    /// <summary>
    /// 기본 ResilienceOptions
    /// </summary>
    public static ResilienceOptions DefaultOptions { get; } = new();

    /// <summary>
    /// AI 작업을 위한 Resilience Pipeline을 생성합니다.
    /// </summary>
    /// <param name="options">복원력 옵션</param>
    /// <returns>구성된 ResiliencePipeline</returns>
    public static ResiliencePipeline<T> Create<T>(ResilienceOptions? options = null)
    {
        options ??= DefaultOptions;

        if (!options.Enabled)
        {
            return ResiliencePipeline<T>.Empty;
        }

        var builder = new ResiliencePipelineBuilder<T>();

        // 타임아웃 (가장 바깥쪽)
        if (options.Timeout.Enabled)
        {
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = options.Timeout.TotalTimeout,
                OnTimeout = args =>
                {
                    // 타임아웃 발생 시 로깅 가능
                    return default;
                }
            });
        }

        // Circuit Breaker
        if (options.CircuitBreaker.Enabled)
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = options.CircuitBreaker.FailureRatio,
                SamplingDuration = options.CircuitBreaker.SamplingDuration,
                MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                BreakDuration = options.CircuitBreaker.BreakDuration,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>()
            });
        }

        // 재시도 (가장 안쪽)
        if (options.Retry.Enabled)
        {
            builder.AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = options.Retry.MaxRetries,
                Delay = options.Retry.InitialDelay,
                MaxDelay = options.Retry.MaxDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = options.Retry.UseJitter,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>(ex => IsTransientHttpError(ex))
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            });
        }

        return builder.Build();
    }

    /// <summary>
    /// 비동기 작업용 Resilience Pipeline을 생성합니다.
    /// </summary>
    public static ResiliencePipeline CreateAsync(ResilienceOptions? options = null)
    {
        options ??= DefaultOptions;

        if (!options.Enabled)
        {
            return ResiliencePipeline.Empty;
        }

        var builder = new ResiliencePipelineBuilder();

        if (options.Timeout.Enabled)
        {
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = options.Timeout.TotalTimeout
            });
        }

        if (options.CircuitBreaker.Enabled)
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = options.CircuitBreaker.FailureRatio,
                SamplingDuration = options.CircuitBreaker.SamplingDuration,
                MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                BreakDuration = options.CircuitBreaker.BreakDuration,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>()
            });
        }

        if (options.Retry.Enabled)
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.Retry.MaxRetries,
                Delay = options.Retry.InitialDelay,
                MaxDelay = options.Retry.MaxDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = options.Retry.UseJitter,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>(ex => IsTransientHttpError(ex))
                    .Handle<TimeoutRejectedException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            });
        }

        return builder.Build();
    }

    private static bool IsTransientHttpError(HttpRequestException ex)
    {
        // 일시적 오류로 간주되는 HTTP 상태 코드
        if (ex.StatusCode.HasValue)
        {
            return ex.StatusCode.Value switch
            {
                HttpStatusCode.RequestTimeout => true,         // 408
                HttpStatusCode.TooManyRequests => true,        // 429
                HttpStatusCode.InternalServerError => true,    // 500
                HttpStatusCode.BadGateway => true,             // 502
                HttpStatusCode.ServiceUnavailable => true,     // 503
                HttpStatusCode.GatewayTimeout => true,         // 504
                _ => false
            };
        }

        // 상태 코드가 없으면 네트워크 오류로 간주하고 재시도
        return true;
    }
}
