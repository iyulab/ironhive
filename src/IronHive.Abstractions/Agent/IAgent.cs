using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Agent;

/// <summary>
/// 에이전트를 나타냅니다.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// 에이전트가 기본으로 사용할 모델 제공자(서비스 키)입니다.
    /// </summary>
    string Provider { get; set; }

    /// <summary>
    /// 에이전트가 기본으로 사용할 모델 이름입니다.
    /// </summary>
    string Model { get; set; }

    /// <summary>
    /// 에이전트의 이름입니다.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 에이전트에 대한 설명입니다.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// 에이전트 동작을 안내하는 지침(시스템 프롬프트 등)입니다.
    /// </summary>
    string? Instruction { get; set; }

    /// <summary>
    /// 에이전트가 사용할 수 있는 도구의 서비스 키 목록입니다.
    /// </summary>
    IEnumerable<ToolItem>? Tools { get; set; }

    /// <summary>
    /// 텍스트 생성(추론) 매개변수입니다. (예: 온도, 최대 토큰 수 등)
    /// </summary>
    MessageGenerationParameters? Parameters { get; set; }

    /// <summary>
    /// 메시지를 처리합니다.
    /// </summary>
    Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 메시지를 스트리밍 방식으로 처리합니다.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}
