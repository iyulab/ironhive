using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// 메시지 생성기를 관리하는 컬렉션입니다.
    /// </summary>
    IKeyedCollection<IMessageGenerator> Generators { get; }

    /// <summary>
    /// 툴을 관리하는 컬렉션입니다.
    /// </summary>
    IKeyedCollection<ITool> Tools { get; }

    /// <summary>
    /// 주어진 요청을 기반으로 채팅 응답 메시지를 생성합니다.
    /// </summary>
    Task<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 주어진 요청을 기반으로 스트리밍 방식의 채팅 응답 메시지를 생성합니다.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);
}
