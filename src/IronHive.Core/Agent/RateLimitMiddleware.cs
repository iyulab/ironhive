using System.Collections.Concurrent;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// API 호출 빈도를 제한하는 미들웨어입니다.
/// 슬라이딩 윈도우 방식으로 rate limit을 적용합니다.
/// </summary>
public class RateLimitMiddleware : IAgentMiddleware
{
    private readonly RateLimitMiddlewareOptions _options;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RateLimitMiddleware(RateLimitMiddlewareOptions? options = null)
    {
        _options = options ?? new RateLimitMiddlewareOptions();
    }

    /// <summary>
    /// 요청 수와 윈도우 시간만 지정하여 생성합니다.
    /// </summary>
    public RateLimitMiddleware(int maxRequests, TimeSpan window)
        : this(new RateLimitMiddlewareOptions { MaxRequests = maxRequests, Window = window })
    {
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        await WaitForSlotAsync(agent.Name, cancellationToken).ConfigureAwait(false);

        try
        {
            return await next(messages).ConfigureAwait(false);
        }
        finally
        {
            // 요청 완료 시간 기록
            _requestTimestamps.Enqueue(DateTime.UtcNow);
        }
    }

    private async Task WaitForSlotAsync(string agentName, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // 윈도우 밖의 오래된 타임스탬프 제거
                var cutoff = DateTime.UtcNow - _options.Window;
                while (_requestTimestamps.TryPeek(out var oldest) && oldest < cutoff)
                {
                    _requestTimestamps.TryDequeue(out _);
                }

                // 슬롯 확인
                if (_requestTimestamps.Count < _options.MaxRequests)
                {
                    return; // 슬롯 확보
                }

                // 다음 슬롯까지 대기 시간 계산
                if (_requestTimestamps.TryPeek(out var nextSlotTime))
                {
                    var waitTime = nextSlotTime + _options.Window - DateTime.UtcNow;
                    if (waitTime > TimeSpan.Zero)
                    {
                        _options.OnRateLimited?.Invoke(agentName, waitTime);

                        // semaphore 해제 후 대기
                        _semaphore.Release();
                        await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                        continue; // 다시 시도
                    }
                }
            }
            finally
            {
                // WaitForSlotAsync 성공 시 semaphore는 해제된 상태로 유지
                if (_semaphore.CurrentCount == 0)
                    _semaphore.Release();
            }
        }
    }

    /// <summary>
    /// 현재 윈도우 내 요청 수를 반환합니다.
    /// </summary>
    public int CurrentRequestCount
    {
        get
        {
            var cutoff = DateTime.UtcNow - _options.Window;
            return _requestTimestamps.Count(t => t >= cutoff);
        }
    }
}

/// <summary>
/// RateLimitMiddleware 설정 옵션
/// </summary>
public class RateLimitMiddlewareOptions
{
    /// <summary>
    /// 윈도우 내 최대 요청 수 (기본값: 60)
    /// </summary>
    public int MaxRequests { get; set; } = 60;

    /// <summary>
    /// 슬라이딩 윈도우 시간 (기본값: 1분)
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Rate limit으로 대기 시 호출되는 콜백 (agentName, waitTime)
    /// </summary>
    public Action<string, TimeSpan>? OnRateLimited { get; set; }
}
