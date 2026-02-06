using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 서비스를 정의하는 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>애플리케이션 코드</b>가 사용합니다.
/// 여러 프로바이더를 통합하고 도구 실행 루프를 관리하는 고수준 오케스트레이터입니다.
/// </para>
/// <para>
/// 프로바이더 구현자는 <see cref="IMessageGenerator"/>를 구현하세요.
/// </para>
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
