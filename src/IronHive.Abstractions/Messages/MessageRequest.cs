using IronHive.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지 생성 요청을 위한 상세 매개변수를 포함하는 클래스입니다.
/// </summary>
public class MessageRequest : MessageGenerationParameters
{
    /// <summary>
    /// 메시지 생성에 사용될 AI 서비스 제공자의 식별자입니다.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// 사용할 특정 모델의 식별자입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 대화의 컨텍스트와 동작을 정의하는 시스템 프롬프트입니다.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// 모델에 전달될 대화 메시지 컬렉션입니다.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// 모델이 사용 가능한 도구들의 목록입니다.
    /// </summary>
    public IEnumerable<ToolItem> Tools { get; set; } = [];

    /// <summary>
    /// 모델의 사고 노력 수준을 정의합니다.
    /// 설정된 경우 모델의 추론 깊이를 조절할 수 있습니다.
    /// </summary>
    public MessageThinkingEffort ThinkingEffort { get; set; } = MessageThinkingEffort.None;

    /// <summary>
    /// 도구 사용에 의한 반복 루프의 최대 반복 횟수를 제한합니다.
    /// 무한 루프를 방지하고 연산을 제어하는 안전장치입니다.
    /// </summary>
    [JsonIgnore]
    public int MaxLoopCount { get; set; } = 10;
}
