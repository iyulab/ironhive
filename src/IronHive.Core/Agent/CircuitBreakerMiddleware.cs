using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 연속 실패 시 회로를 차단하여 시스템을 보호하는 미들웨어입니다.
/// Circuit Breaker 패턴을 구현합니다.
/// </summary>
public class CircuitBreakerMiddleware : IAgentMiddleware
{
    private readonly CircuitBreakerMiddlewareOptions _options;
    private readonly object _lock = new();
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private DateTime _openedAt;

    public CircuitBreakerMiddleware(CircuitBreakerMiddlewareOptions? options = null)
    {
        _options = options ?? new CircuitBreakerMiddlewareOptions();
    }

    /// <summary>
    /// 실패 임계값과 차단 시간만 지정하여 생성합니다.
    /// </summary>
    public CircuitBreakerMiddleware(int failureThreshold, TimeSpan breakDuration)
        : this(new CircuitBreakerMiddlewareOptions
        {
            FailureThreshold = failureThreshold,
            BreakDuration = breakDuration
        })
    {
    }

    /// <summary>
    /// 현재 회로 상태
    /// </summary>
    public CircuitState State
    {
        get
        {
            lock (_lock)
            {
                UpdateState();
                return _state;
            }
        }
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            UpdateState();

            if (_state == CircuitState.Open)
            {
                _options.OnRejected?.Invoke(agent.Name, _state);
                throw new CircuitBreakerOpenException(
                    $"Circuit breaker is open for agent '{agent.Name}'. " +
                    $"Will retry after {(_openedAt + _options.BreakDuration - DateTime.UtcNow).TotalSeconds:F1}s.");
            }
        }

        try
        {
            var response = await next(messages).ConfigureAwait(false);

            lock (_lock)
            {
                OnSuccess();
            }

            return response;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            lock (_lock)
            {
                OnFailure(agent.Name, ex);
            }
            throw;
        }
    }

    private void UpdateState()
    {
        if (_state == CircuitState.Open)
        {
            // Break duration이 지났으면 Half-Open으로 전환
            if (DateTime.UtcNow >= _openedAt + _options.BreakDuration)
            {
                _state = CircuitState.HalfOpen;
                _options.OnStateChanged?.Invoke(CircuitState.Open, CircuitState.HalfOpen);
            }
        }
        else if (_state == CircuitState.Closed)
        {
            // 실패 윈도우가 지났으면 실패 카운트 리셋
            if (_failureCount > 0 && DateTime.UtcNow >= _lastFailureTime + _options.FailureWindow)
            {
                _failureCount = 0;
            }
        }
    }

    private void OnSuccess()
    {
        if (_state == CircuitState.HalfOpen)
        {
            // Half-Open에서 성공하면 Closed로 전환
            var oldState = _state;
            _state = CircuitState.Closed;
            _failureCount = 0;
            _options.OnStateChanged?.Invoke(oldState, _state);
        }
        else if (_state == CircuitState.Closed)
        {
            // Closed에서 성공하면 실패 카운트 리셋
            _failureCount = 0;
        }
    }

    private void OnFailure(string agentName, Exception ex)
    {
        _lastFailureTime = DateTime.UtcNow;

        if (_state == CircuitState.HalfOpen)
        {
            // Half-Open에서 실패하면 다시 Open
            var oldState = _state;
            _state = CircuitState.Open;
            _openedAt = DateTime.UtcNow;
            _options.OnStateChanged?.Invoke(oldState, _state);
            _options.OnBreak?.Invoke(agentName, ex, _options.BreakDuration);
        }
        else if (_state == CircuitState.Closed)
        {
            _failureCount++;

            if (_failureCount >= _options.FailureThreshold)
            {
                // 임계값 도달 시 Open
                var oldState = _state;
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                _options.OnStateChanged?.Invoke(oldState, _state);
                _options.OnBreak?.Invoke(agentName, ex, _options.BreakDuration);
            }
        }
    }

    /// <summary>
    /// 회로를 수동으로 리셋합니다.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            var oldState = _state;
            _state = CircuitState.Closed;
            _failureCount = 0;
            if (oldState != CircuitState.Closed)
            {
                _options.OnStateChanged?.Invoke(oldState, CircuitState.Closed);
            }
        }
    }
}

/// <summary>
/// Circuit Breaker 상태
/// </summary>
public enum CircuitState
{
    /// <summary>정상 상태 - 요청 허용</summary>
    Closed,
    /// <summary>차단 상태 - 요청 거부</summary>
    Open,
    /// <summary>반개방 상태 - 테스트 요청 허용</summary>
    HalfOpen
}

/// <summary>
/// Circuit Breaker가 Open 상태일 때 발생하는 예외
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
    public CircuitBreakerOpenException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// CircuitBreakerMiddleware 설정 옵션
/// </summary>
public class CircuitBreakerMiddlewareOptions
{
    /// <summary>
    /// 회로를 Open하기 위한 연속 실패 횟수 (기본값: 5)
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// 실패 카운트가 유지되는 윈도우 시간 (기본값: 1분)
    /// 이 시간 내에 FailureThreshold만큼 실패해야 회로가 Open됨
    /// </summary>
    public TimeSpan FailureWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 회로가 Open 상태로 유지되는 시간 (기본값: 30초)
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 회로가 Open될 때 호출되는 콜백 (agentName, lastException, breakDuration)
    /// </summary>
    public Action<string, Exception, TimeSpan>? OnBreak { get; set; }

    /// <summary>
    /// 상태가 변경될 때 호출되는 콜백 (oldState, newState)
    /// </summary>
    public Action<CircuitState, CircuitState>? OnStateChanged { get; set; }

    /// <summary>
    /// 요청이 거부될 때 호출되는 콜백 (agentName, currentState)
    /// </summary>
    public Action<string, CircuitState>? OnRejected { get; set; }
}
