using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// LLM 프로바이더의 메시지 생성 기능을 정의하는 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>프로바이더 구현자</b>가 사용합니다.
/// OpenAI, Anthropic 등 특정 LLM 제공자에 대한 저수준 어댑터 역할을 합니다.
/// </para>
/// <para>
/// 애플리케이션 코드에서는 <see cref="IMessageService"/>를 사용하세요.
/// </para>
/// </summary>
public interface IMessageGenerator : IProviderItem
{
    /// <summary>
    /// 주어진 요청을 기반으로 채팅 응답 메시지를 생성합니다.
    /// </summary>
    Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 주어진 요청을 기반으로 스트리밍 방식의 채팅 응답 메시지를 생성합니다.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default);
}