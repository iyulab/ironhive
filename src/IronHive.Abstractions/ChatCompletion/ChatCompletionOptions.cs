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
    /// 사용할 툴의 이름과 옵션입니다.
    /// </summary>
    public IDictionary<string, object?>? Tools { get; set; }
}
