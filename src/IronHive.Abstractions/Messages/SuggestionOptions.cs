namespace IronHive.Abstractions.Messages;

public enum SuggestionMode
{
    /// <summary>유저에게 도움이 될 때만 포함합니다.</summary>
    Auto,
    /// <summary>항상 포함합니다.</summary>
    Always,
}

/// <summary>
/// 제안 기능 활성화 및 설정 옵션입니다.
/// MessageRequest.Suggestions에 null이 아닌 값을 지정하면 기능이 활성화됩니다.
/// </summary>
public class SuggestionOptions
{
    /// <summary>
    /// 제안 포함 방식입니다.
    /// Auto이면 모델이 판단해 생략할 수 있고, Always이면 항상 포함합니다.
    /// </summary>
    public SuggestionMode Mode { get; set; } = SuggestionMode.Auto;

    /// <summary>
    /// 응답당 제안 블록의 최대 개수입니다.
    /// </summary>
    public int MaxCount { get; set; } = 1;

    /// <summary>
    /// 각 제안 블록당 항목의 최소 개수입니다.
    /// </summary>
    public int MinItems { get; set; } = 2;

    /// <summary>
    /// 각 제안 블록당 항목의 최대 개수입니다.
    /// </summary>
    public int MaxItems { get; set; } = 4;
}
