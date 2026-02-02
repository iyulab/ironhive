using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 요청을 위한 상세 매개변수를 포함하는 클래스입니다.
/// </summary>
public class MessageGenerationRequest : MessageGenerationParameters
{
    /// <summary>
    /// 사용할 특정 모델의 식별자입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 대화의 컨텍스트와 동작을 정의하는 시스템 프롬프트입니다.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// 대화의 컨텍스트와 동작을 정의하는 시스템 프롬프트입니다.
    /// </summary>
    [Obsolete("Use SystemPrompt instead. Will be removed in v1.0.")]
    public string? System
    {
        get => SystemPrompt;
        set => SystemPrompt = value;
    }

    /// <summary>
    /// 모델에 전달될 대화 메시지 컬렉션입니다.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// 모델에서 사용 가능한 도구(툴) 목록입니다.
    /// </summary>
    public IToolCollection? Tools { get; set; }

    /// <summary>
    /// 모델의 사고 노력 수준을 정의합니다.
    /// 설정된 경우 모델의 추론 깊이를 조절할 수 있습니다.
    /// </summary>
    public MessageThinkingEffort? ThinkingEffort { get; set; }
}
