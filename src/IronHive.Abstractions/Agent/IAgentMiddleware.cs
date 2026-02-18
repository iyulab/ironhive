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
#pragma warning disable CA1716 // Identifiers should not match keywords — 'next' is the standard middleware pipeline convention
    Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
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
#pragma warning disable CA1716 // Identifiers should not match keywords — 'next' is the standard middleware pipeline convention
    IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default);
#pragma warning restore CA1716
}
