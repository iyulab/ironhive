using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 실패 시 대체 에이전트로 폴백하는 미들웨어입니다.
/// 스트리밍과 비스트리밍 모두 지원합니다 — 스트리밍 호출은 1차 에이전트가 <b>아직 한 프레임도 내보내지
/// 않았을 때</b> 실패한 경우에만 대체 에이전트의 스트림으로 넘어갑니다. 완성된 응답이 있어야 판단할 수 있는
/// <see cref="FallbackMiddlewareOptions.ResponseValidator"/>는 스트리밍 호출에 적용되지 않습니다.
/// </summary>
public class FallbackMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
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
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await next(messages, options).ConfigureAwait(false);

            // 응답 검증 (선택적)
            if (_options.ResponseValidator != null && !_options.ResponseValidator(response))
            {
                _options.OnFallback?.Invoke(agent.Name, null, "Response validation failed");
                return await ExecuteFallbackAsync(agent, messages, options, cancellationToken).ConfigureAwait(false);
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
            return await ExecuteFallbackAsync(agent, messages, options, cancellationToken).ConfigureAwait(false);
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
        // The fallback agent re-reads the input, so it is materialized once.
        var messageList = messages.ToList();
        Exception? primaryFailure = null;

        var primary = next(messageList, options).GetAsyncEnumerator(cancellationToken);
        try
        {
            bool hasFirst = false;
            try
            {
                hasFirst = await primary.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                                       && (_options.ShouldFallback?.Invoke(ex) ?? true))
            {
                primaryFailure = ex;
            }

            if (primaryFailure is null)
            {
                if (!hasFirst)
                {
                    yield break;
                }

                // From here on the caller has frames in hand: a later failure is theirs to see.
                yield return primary.Current;
                while (await primary.MoveNextAsync().ConfigureAwait(false))
                {
                    yield return primary.Current;
                }

                yield break;
            }
        }
        finally
        {
            await primary.DisposeAsync().ConfigureAwait(false);
        }

        _options.OnFallback?.Invoke(agent.Name, primaryFailure, primaryFailure.Message);
        var fallbackAgent = ResolveFallbackAgent(agent);

        var fallback = fallbackAgent.InvokeStreamingAsync(messageList, options, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = await fallback.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception fallbackEx) when (!cancellationToken.IsCancellationRequested)
                {
                    throw FallbackFailed(agent, fallbackAgent, fallbackEx);
                }

                if (!hasNext)
                {
                    yield break;
                }

                yield return fallback.Current;
            }
        }
        finally
        {
            await fallback.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<MessageResponse> ExecuteFallbackAsync(
        IAgent primaryAgent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        CancellationToken cancellationToken)
    {
        var fallbackAgent = ResolveFallbackAgent(primaryAgent);

        try
        {
            return await fallbackAgent.InvokeAsync(messages, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception fallbackEx) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller's own cancellation is not a fallback failure: it propagates as itself.
            throw FallbackFailed(primaryAgent, fallbackAgent, fallbackEx);
        }
    }

    private IAgent ResolveFallbackAgent(IAgent primaryAgent)
        => _options.FallbackAgent ?? _options.FallbackFactory!(primaryAgent);

    private FallbackFailedException FallbackFailed(IAgent primaryAgent, IAgent fallbackAgent, Exception fallbackEx)
    {
        _options.OnFallbackFailed?.Invoke(fallbackAgent.Name, fallbackEx);
        return new FallbackFailedException(
            $"Both primary agent '{primaryAgent.Name}' and fallback agent '{fallbackAgent.Name}' failed.",
            fallbackEx);
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
    /// 응답 검증 함수 (선택적). false를 반환하면 폴백 실행.
    /// 완성된 응답이 필요하므로 버퍼드 호출(<c>InvokeAsync</c>)에만 적용됩니다.
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
