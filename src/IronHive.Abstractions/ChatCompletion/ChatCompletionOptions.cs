using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.ChatCompletion;

public class ChatCompletionOptions : ChatCompletionParameters
{
    /// <summary>
    /// 모델 제공자의 키 값입니다.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// 모델의 이름(또는 식별자)입니다.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 시스템 메시지로 사용할 텍스트입니다.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// 사용할 툴입니다.
    /// </summary>
    public ToolCollection? Tools { get; set; }

    /// <summary>
    /// 툴 사용에 의한 루프를 최대 몇번까지 돌릴지 설정합니다.
    /// </summary>
    public int MaxLoopCount { get; set; } = 10;
}
