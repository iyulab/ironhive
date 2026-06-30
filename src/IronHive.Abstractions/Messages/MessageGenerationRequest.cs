using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 프로바이더(IMessageGenerator)에 전달되는 메시지 생성 요청입니다.
/// </summary>
public class MessageGenerationRequest
{
    /// <summary>
    /// 이전 응답의 ResponseId. 프로바이더 측 저장된 컨텍스트를 재사용해 비용을 절감합니다.
    /// </summary>
    public string? PreviousId { get; set; }

    /// <summary>
    /// 사용할 특정 모델의 식별자입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 모델의 사고 노력 수준을 정의합니다.
    /// </summary>
    public MessageThinkingEffort? ThinkingEffort { get; set; }

    /// <summary>
    /// 생성할 최대 토큰 수입니다.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// 대화의 컨텍스트와 동작을 정의하는 시스템 프롬프트입니다.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// 모델에 전달될 대화 메시지 컬렉션입니다.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// 모델에서 사용 가능한 도구(툴) 목록입니다.
    /// </summary>
    public IToolCollection? Tools { get; set; }

    /// <summary>
    /// 구조화 출력 설정입니다. null이면 기본 텍스트 출력입니다.
    /// </summary>
    public OutputOptions? Output { get; set; }
}
