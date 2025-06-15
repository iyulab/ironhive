using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Message;

public class MessageGenerationRequest
{
    /// <summary>
    /// 모델 제공자의 키 값입니다.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// chat completion model identifier.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// system message to generate a response to.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// messages to generate a response to.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// the tool list to use in the model.
    /// </summary>
    public IEnumerable<ToolDescriptor> Tools { get; set; } = [];

    /// <summary>
    /// when the model can thinking, this property will be set.
    /// </summary>
    public MessageThinkingEffort? ThinkingEffort { get; set; }

    /// <summary>
    /// the parameters for message generation.
    /// </summary>
    public MessageGenerationParameters? Parameters { get; set; }

    /// <summary>
    /// 툴 사용에 의한 루프를 최대 몇번까지 돌릴지 설정합니다.
    /// </summary>
    public int MaxLoopCount { get; set; } = 10;
}
