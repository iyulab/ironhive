using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 실패 시 대체 에이전트로 폴백하는 미들웨어입니다.
/// </summary>
public class FallbackMiddleware : IAgentMiddleware
{
    private readonly FallbackMiddlewareOptions _options;

    public FallbackMiddleware(FallbackMiddlewareOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (_options.FallbackAgent == null && _options.FallbackFactory == null)
        {
            throw new ArgumentException(
                "Either FallbackAgent or FallbackFactory must be provided.",
                nameof(options));
        }
    }

    /// <summary>
    /// 폴백 에이전트만 지정하여 생성합니다.
    /// </summary>
    public FallbackMiddleware(IAgent fallbackAgent)
        : this(new FallbackMiddlewareOptions { FallbackAgent = fallbackAgent })
    {
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await next(messages).ConfigureAwait(false);

            // 응답 검증 (선택적)
            if (_options.ResponseValidator != null && !_options.ResponseValidator(response))
            {
                _options.OnFallback?.Invoke(agent.Name, null, "Response validation failed");
                return await ExecuteFallbackAsync(agent, messages, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // 폴백 조건 확인
            if (_options.ShouldFallback != null && !_options.ShouldFallback(ex))
            {
                throw;
            }

            _options.OnFallback?.Invoke(agent.Name, ex, ex.Message);
            return await ExecuteFallbackAsync(agent, messages, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<MessageResponse> ExecuteFallbackAsync(
        IAgent primaryAgent,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var fallbackAgent = _options.FallbackAgent
            ?? _options.FallbackFactory!(primaryAgent);

        try
        {
            return await fallbackAgent.InvokeAsync(messages, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception fallbackEx)
        {
            _options.OnFallbackFailed?.Invoke(fallbackAgent.Name, fallbackEx);
            throw new FallbackFailedException(
                $"Both primary agent '{primaryAgent.Name}' and fallback agent '{fallbackAgent.Name}' failed.",
                fallbackEx);
        }
    }
}

/// <summary>
/// 폴백도 실패했을 때 발생하는 예외
/// </summary>
public class FallbackFailedException : Exception
{
    public FallbackFailedException(string message) : base(message) { }
    public FallbackFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// FallbackMiddleware 설정 옵션
/// </summary>
public class FallbackMiddlewareOptions
{
    /// <summary>
    /// 대체 에이전트
    /// </summary>
    public IAgent? FallbackAgent { get; set; }

    /// <summary>
    /// 대체 에이전트를 동적으로 생성하는 팩토리
    /// primaryAgent를 참조하여 적절한 폴백을 선택할 수 있음
    /// </summary>
    public Func<IAgent, IAgent>? FallbackFactory { get; set; }

    /// <summary>
    /// 폴백 조건 (기본: 모든 예외에서 폴백)
    /// false를 반환하면 예외가 그대로 전파됨
    /// </summary>
    public Func<Exception, bool>? ShouldFallback { get; set; }

    /// <summary>
    /// 응답 검증 함수 (선택적)
    /// false를 반환하면 폴백 실행
    /// </summary>
    public Func<MessageResponse, bool>? ResponseValidator { get; set; }

    /// <summary>
    /// 폴백 발생 시 호출되는 콜백 (primaryAgentName, exception, reason)
    /// </summary>
    public Action<string, Exception?, string>? OnFallback { get; set; }

    /// <summary>
    /// 폴백도 실패했을 때 호출되는 콜백 (fallbackAgentName, exception)
    /// </summary>
    public Action<string, Exception>? OnFallbackFailed { get; set; }
}
