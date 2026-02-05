using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 호출 실패 시 지수 백오프로 재시도하는 미들웨어입니다.
/// </summary>
public class RetryMiddleware : IAgentMiddleware
{
    private readonly RetryMiddlewareOptions _options;

    public RetryMiddleware(RetryMiddlewareOptions? options = null)
    {
        _options = options ?? new RetryMiddlewareOptions();
    }

    /// <summary>
    /// 최대 재시도 횟수만 지정하여 생성합니다.
    /// </summary>
    public RetryMiddleware(int maxRetries)
        : this(new RetryMiddlewareOptions { MaxRetries = maxRetries })
    {
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var attempts = 0;
        var delay = _options.InitialDelay;
        Exception? lastException = null;

        while (attempts <= _options.MaxRetries)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await next(messages).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw; // 취소는 재시도하지 않음
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempts++;

                // 재시도 가능 여부 확인
                if (attempts > _options.MaxRetries || !_options.ShouldRetry(ex))
                {
                    _options.OnRetryFailed?.Invoke(agent.Name, attempts - 1, ex);
                    throw;
                }

                // 재시도 콜백 호출
                _options.OnRetry?.Invoke(agent.Name, attempts, ex, delay);

                // 지터가 포함된 지수 백오프 대기
                var jitteredDelay = ApplyJitter(delay);
                await Task.Delay(jitteredDelay, cancellationToken).ConfigureAwait(false);

                // 다음 재시도를 위한 딜레이 증가
                delay = TimeSpan.FromMilliseconds(
                    Math.Min(delay.TotalMilliseconds * _options.BackoffMultiplier,
                             _options.MaxDelay.TotalMilliseconds));
            }
        }

        // 이 지점에 도달하면 안 됨
        throw lastException ?? new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private TimeSpan ApplyJitter(TimeSpan delay)
    {
        if (_options.JitterFactor <= 0)
            return delay;

        var jitterRange = delay.TotalMilliseconds * _options.JitterFactor;
        var jitter = (Random.Shared.NextDouble() * 2 - 1) * jitterRange;
        var jitteredMs = Math.Max(0, delay.TotalMilliseconds + jitter);
        return TimeSpan.FromMilliseconds(jitteredMs);
    }
}

/// <summary>
/// RetryMiddleware 설정 옵션
/// </summary>
public class RetryMiddlewareOptions
{
    /// <summary>
    /// 최대 재시도 횟수 (기본값: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 초기 대기 시간 (기본값: 1초)
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 최대 대기 시간 (기본값: 30초)
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 백오프 배수 (기본값: 2.0)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// 지터 비율 (0~1, 기본값: 0.2)
    /// 실제 딜레이에 ±JitterFactor 범위의 랜덤 변동을 추가합니다.
    /// </summary>
    public double JitterFactor { get; set; } = 0.2;

    /// <summary>
    /// 재시도 가능 여부 판단 함수.
    /// 기본값은 모든 예외에 대해 재시도합니다.
    /// </summary>
    public Func<Exception, bool> ShouldRetry { get; set; } = _ => true;

    /// <summary>
    /// 재시도 시 호출되는 콜백 (agentName, attemptNumber, exception, delay)
    /// </summary>
    public Action<string, int, Exception, TimeSpan>? OnRetry { get; set; }

    /// <summary>
    /// 모든 재시도 실패 후 호출되는 콜백 (agentName, totalAttempts, lastException)
    /// </summary>
    public Action<string, int, Exception>? OnRetryFailed { get; set; }
}
