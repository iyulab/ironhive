using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent;

/// <summary>
/// 에이전트 실행 전후에 인터셉션하는 미들웨어입니다.
/// </summary>
public interface IAgentMiddleware
{
    /// <summary>
    /// 에이전트 실행을 인터셉트합니다.
    /// next를 호출하지 않으면 short-circuit됩니다.
    /// </summary>
    /// <param name="agent">실행 대상 에이전트입니다.</param>
    /// <param name="messages">모델에 전달될 대화 메시지 컬렉션입니다.</param>
    /// <param name="options">이 호출의 per-request 옵션입니다. next로 전달하거나 수정할 수 있습니다.</param>
    /// <param name="next">다음 미들웨어 또는 최종 에이전트 호출입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
#pragma warning disable CA1716 // Identifiers should not match keywords — 'next' is the standard middleware pipeline convention
    Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default);
#pragma warning restore CA1716
}

/// <summary>
/// 스트리밍 에이전트 실행을 인터셉션하는 미들웨어입니다.
/// IAgentMiddleware와 함께 구현하면 스트리밍과 비스트리밍 모두 지원합니다.
/// </summary>
public interface IStreamingAgentMiddleware
{
    /// <summary>
    /// 스트리밍 에이전트 실행을 인터셉트합니다.
    /// </summary>
    /// <param name="agent">실행 대상 에이전트입니다.</param>
    /// <param name="messages">모델에 전달될 대화 메시지 컬렉션입니다.</param>
    /// <param name="options">이 호출의 per-request 옵션입니다. next로 전달하거나 수정할 수 있습니다.</param>
    /// <param name="next">다음 미들웨어 또는 최종 에이전트 호출입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
#pragma warning disable CA1716 // Identifiers should not match keywords — 'next' is the standard middleware pipeline convention
    IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        AgentInvokeOptions? options,
        Func<IEnumerable<Message>, AgentInvokeOptions?, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default);
#pragma warning restore CA1716
}
