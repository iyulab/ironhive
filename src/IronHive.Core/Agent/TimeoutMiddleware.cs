using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 호출에 타임아웃을 적용하는 미들웨어입니다.
/// </summary>
public class TimeoutMiddleware : IAgentMiddleware
{
    private readonly TimeoutMiddlewareOptions _options;

    public TimeoutMiddleware(TimeoutMiddlewareOptions? options = null)
    {
        _options = options ?? new TimeoutMiddlewareOptions();
    }

    /// <summary>
    /// 타임아웃만 지정하여 생성합니다.
    /// </summary>
    public TimeoutMiddleware(TimeSpan timeout)
        : this(new TimeoutMiddlewareOptions { Timeout = timeout })
    {
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_options.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        try
        {
            // 타임아웃 CTS를 사용하여 실행
            // next는 원래 cancellationToken을 사용하므로, 래퍼로 타임아웃 적용
            var task = next(messages);
            var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, linkedCts.Token))
                .ConfigureAwait(false);

            if (completedTask == task)
            {
                return await task.ConfigureAwait(false);
            }

            // 타임아웃 발생
            _options.OnTimeout?.Invoke(agent.Name, _options.Timeout);
            throw new TimeoutException(
                $"Agent '{agent.Name}' timed out after {_options.Timeout.TotalSeconds:F1}s.");
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // 타임아웃으로 인한 취소
            _options.OnTimeout?.Invoke(agent.Name, _options.Timeout);
            throw new TimeoutException(
                $"Agent '{agent.Name}' timed out after {_options.Timeout.TotalSeconds:F1}s.");
        }
    }
}

/// <summary>
/// TimeoutMiddleware 설정 옵션
/// </summary>
public class TimeoutMiddlewareOptions
{
    /// <summary>
    /// 타임아웃 시간 (기본값: 30초)
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 타임아웃 발생 시 호출되는 콜백 (agentName, timeout)
    /// </summary>
    public Action<string, TimeSpan>? OnTimeout { get; set; }
}
