using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 격리된 실행 풀로 리소스 고갈을 방지하는 미들웨어입니다.
/// Bulkhead 패턴을 구현하여 동시 실행 수를 제한합니다.
/// 스트리밍과 비스트리밍 모두 지원합니다 — 스트리밍 호출은 첫 프레임 전에 실행 슬롯을 얻고,
/// 스트림이 끝날 때(완료 · 예외 · 호출자의 조기 중단) 반납합니다.
/// </summary>
public class BulkheadMiddleware : IAgentMiddleware, IStreamingAgentMiddleware, IDisposable
{
    private readonly BulkheadMiddlewareOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private readonly SemaphoreSlim? _queueSemaphore;
    private int _currentExecuting;
    private int _currentQueued;

    public BulkheadMiddleware(BulkheadMiddlewareOptions? options = null)
    {
        _options = options ?? new BulkheadMiddlewareOptions();
        _semaphore = new SemaphoreSlim(_options.MaxConcurrency, _options.MaxConcurrency);

        if (_options.MaxQueueSize > 0)
        {
            _queueSemaphore = new SemaphoreSlim(_options.MaxQueueSize, _options.MaxQueueSize);
        }
    }

    /// <summary>
    /// 동시 실행 수만 지정하여 생성합니다.
    /// </summary>
    public BulkheadMiddleware(int maxConcurrency)
        : this(new BulkheadMiddlewareOptions { MaxConcurrency = maxConcurrency })
    {
    }

    /// <summary>
    /// 동시 실행 수와 대기열 크기를 지정하여 생성합니다.
    /// </summary>
    public BulkheadMiddleware(int maxConcurrency, int maxQueueSize)
        : this(new BulkheadMiddlewareOptions
        {
            MaxConcurrency = maxConcurrency,
            MaxQueueSize = maxQueueSize
        })
    {
    }

    /// <summary>
    /// 현재 실행 중인 요청 수
    /// </summary>
    public int CurrentExecuting => Volatile.Read(ref _currentExecuting);

    /// <summary>
    /// 현재 대기 중인 요청 수
    /// </summary>
    public int CurrentQueued => Volatile.Read(ref _currentQueued);

    /// <summary>
    /// 사용 가능한 실행 슬롯 수
    /// </summary>
    public int AvailableSlots => _semaphore.CurrentCount;

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        await EnterAsync(agent.Name, cancellationToken).ConfigureAwait(false);

        try
        {
            return await next(messages, options).ConfigureAwait(false);
        }
        finally
        {
            Exit();
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
        await EnterAsync(agent.Name, cancellationToken).ConfigureAwait(false);

        try
        {
            await foreach (var frame in next(messages, options).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return frame;
            }
        }
        finally
        {
            Exit();
        }
    }

    /// <summary>
    /// 대기열 슬롯(제한이 있으면)을 거쳐 실행 슬롯을 얻습니다. 대기열 슬롯은 실행 슬롯을 얻든 못 얻든
    /// 여기서 반납됩니다 — 실행이 시작된 뒤의 실패는 대기열과 무관하므로, 그 실패가 대기열 회계를
    /// 건드리지 않게 하기 위해서입니다.
    /// </summary>
    private async Task EnterAsync(string agentName, CancellationToken cancellationToken)
    {
        // 대기열 제한이 있는 경우 먼저 대기열 슬롯 확보
        if (_queueSemaphore != null)
        {
            if (!await _queueSemaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            {
                _options.OnRejected?.Invoke(agentName, CurrentExecuting, CurrentQueued);
                throw new BulkheadRejectedException(
                    $"Bulkhead rejected request for agent '{agentName}'. " +
                    $"Queue is full (executing={CurrentExecuting}, queued={CurrentQueued}).");
            }

            Interlocked.Increment(ref _currentQueued);
        }

        try
        {
            // 실행 슬롯 대기
            _options.OnQueued?.Invoke(agentName, CurrentExecuting, CurrentQueued);

            if (!await _semaphore.WaitAsync(_options.QueueTimeout, cancellationToken).ConfigureAwait(false))
            {
                _options.OnRejected?.Invoke(agentName, CurrentExecuting, CurrentQueued);
                throw new BulkheadRejectedException(
                    $"Bulkhead timed out waiting for slot for agent '{agentName}'. " +
                    $"Timeout={_options.QueueTimeout.TotalSeconds:F1}s.");
            }
        }
        finally
        {
            if (_queueSemaphore != null)
            {
                Interlocked.Decrement(ref _currentQueued);
                _queueSemaphore.Release();
            }
        }

        Interlocked.Increment(ref _currentExecuting);
    }

    private void Exit()
    {
        Interlocked.Decrement(ref _currentExecuting);
        _semaphore.Release();
    }

    /// <summary>
    /// 세마포어 리소스를 해제합니다.
    /// </summary>
    public void Dispose()
    {
        _semaphore.Dispose();
        _queueSemaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Bulkhead가 요청을 거부할 때 발생하는 예외
/// </summary>
public class BulkheadRejectedException : Exception
{
    public BulkheadRejectedException(string message) : base(message) { }
    public BulkheadRejectedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// BulkheadMiddleware 설정 옵션
/// </summary>
public class BulkheadMiddlewareOptions
{
    /// <summary>
    /// 최대 동시 실행 수 (기본값: 10)
    /// </summary>
    public int MaxConcurrency { get; set; } = 10;

    /// <summary>
    /// 대기열 최대 크기 (기본값: 0 - 무제한)
    /// 0보다 크면 대기열이 가득 찼을 때 요청을 거부합니다.
    /// </summary>
    public int MaxQueueSize { get; set; }

    /// <summary>
    /// 대기열에서 슬롯을 기다리는 최대 시간 (기본값: 무한대)
    /// </summary>
    public TimeSpan QueueTimeout { get; set; } = Timeout.InfiniteTimeSpan;

    /// <summary>
    /// 요청이 대기열에 들어갈 때 호출되는 콜백 (agentName, executing, queued)
    /// </summary>
    public Action<string, int, int>? OnQueued { get; set; }

    /// <summary>
    /// 요청이 거부될 때 호출되는 콜백 (agentName, executing, queued)
    /// </summary>
    public Action<string, int, int>? OnRejected { get; set; }
}
