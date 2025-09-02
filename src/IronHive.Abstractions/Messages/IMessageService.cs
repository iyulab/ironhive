using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IMessageService
{
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
