namespace IronHive.Abstractions.Messages;

/// <summary>
/// LLM(대규모 언어 모델) 사용 시 토큰 사용량을 나타내는 클래스입니다.
/// </summary>
public class MessageTokenUsage
{
    /// <summary>
    /// 입력에 사용된 토큰 수입니다.
    /// </summary>
    public int InputTokens { get; set; } = 0;

    /// <summary>
    /// 출력에 사용된 토큰 수입니다.
    /// </summary>
    public int OutputTokens { get; set; } = 0;

    /// <summary>
    /// 총 사용된 토큰 수입니다. (입력 + 출력)
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;
}