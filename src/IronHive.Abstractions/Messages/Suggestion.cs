namespace IronHive.Abstractions.Messages;

/// <summary>
/// 모델이 유저에게 제안하는 Q&amp;A 블록입니다.
/// MessageResponse.Suggestions / StreamingMessageDoneResponse.Suggestions 필드로 반환됩니다.
/// </summary>
public class Suggestion
{
    /// <summary>
    /// 모델이 유저에게 던지는 질문입니다.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// 유저가 선택할 수 있는 제안 항목 목록입니다.
    /// </summary>
    public List<string> Items { get; set; } = [];
}
