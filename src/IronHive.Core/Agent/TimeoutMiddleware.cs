using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 호출에 타임아웃을 적용하는 미들웨어입니다.
/// 스트리밍과 비스트리밍 모두 지원하며, 두 경우 모두 <see cref="TimeoutMiddlewareOptions.Timeout"/>은
/// <b>호출 전체</b>의 기한입니다 — 스트리밍에서는 첫 프레임부터 마지막 프레임까지, 호출자가 프레임 사이에
/// 쓰는 시간을 포함합니다. 첫 프레임까지의 대기나 프레임 사이 유휴 시간을 따로 재지 않습니다.
/// </summary>
public class TimeoutMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
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
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_options.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        try
        {
            // 타임아웃 CTS를 사용하여 실행
            // next는 원래 cancellationToken을 사용하므로, 래퍼로 타임아웃 적용
            var task = next(messages, options);
            var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, linkedCts.Token))
                .ConfigureAwait(false);

            if (completedTask == task)
            {
                return await task.ConfigureAwait(false);
            }

            // 타임아웃 발생
            throw TimedOut(agent.Name);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // 타임아웃으로 인한 취소
            throw TimedOut(agent.Name);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, IAsyncEnumerable<StreamingMessageResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_options.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        // A source that honours its token ends on its own when the deadline passes; the race below is
        // the backstop for one that does not (a stalled connection that never yields again). One
        // deadline task serves the whole stream, so waiting on it per frame registers nothing new.
        var enumerator = next(messages, options).GetAsyncEnumerator(linkedCts.Token);
        var deadline = Task.Delay(Timeout.Infinite, linkedCts.Token);
        Task<bool>? abandonedMove = null;

        try
        {
            while (true)
            {
                var move = enumerator.MoveNextAsync().AsTask();
                if (await Task.WhenAny(move, deadline).ConfigureAwait(false) != move)
                {
                    abandonedMove = move;
                    cancellationToken.ThrowIfCancellationRequested();
                    throw TimedOut(agent.Name);
                }

                bool hasNext;
                try
                {
                    hasNext = await move.ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    throw TimedOut(agent.Name);
                }

                if (!hasNext)
                {
                    yield break;
                }

                yield return enumerator.Current;
            }
        }
        finally
        {
            if (abandonedMove is null)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                // Disposing a compiler-generated async iterator while its MoveNextAsync is still in
                // flight throws; release it once that step settles instead.
                DisposeWhenSettled(abandonedMove, enumerator);
            }
        }
    }

    private TimeoutException TimedOut(string agentName)
    {
        _options.OnTimeout?.Invoke(agentName, _options.Timeout);
        return new TimeoutException(
            $"Agent '{agentName}' timed out after {_options.Timeout.TotalSeconds:F1}s.");
    }

    private static void DisposeWhenSettled(Task<bool> move, IAsyncEnumerator<StreamingMessageResponse> enumerator)
    {
        _ = move.ContinueWith(
            static async (settled, state) =>
            {
                _ = settled.Exception; // observed: the caller already has its TimeoutException
                try
                {
                    await ((IAsyncEnumerator<StreamingMessageResponse>)state!).DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Nothing is listening any more; a failure to release an abandoned stream must not
                    // surface as an unobserved task exception.
                }
            },
            enumerator,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default).Unwrap();
    }
}

/// <summary>
/// TimeoutMiddleware 설정 옵션
/// </summary>
public class TimeoutMiddlewareOptions
{
    /// <summary>
    /// 호출 전체의 타임아웃 (기본값: 30초). 스트리밍 호출에서도 첫 프레임까지가 아니라 스트림 전체에 적용됩니다.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 타임아웃 발생 시 호출되는 콜백 (agentName, timeout)
    /// </summary>
    public Action<string, TimeSpan>? OnTimeout { get; set; }
}
