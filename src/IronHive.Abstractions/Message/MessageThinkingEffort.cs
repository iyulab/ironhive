namespace IronHive.Abstractions.Message;

public enum MessageThinkingEffort
{
    /// <summary>
    /// 최소한의 생각을 요구합니다. 빠른 응답이 필요할 때 사용합니다.
    /// </summary>
    Low,

    /// <summary>
    /// 일반적인 생각을 요구합니다. 대부분의 상황에서 적절한 선택입니다.
    /// </summary>
    Medium,

    /// <summary>
    /// 깊은 생각을 요구합니다. 복잡한 문제를 해결할 때 사용합니다.
    /// </summary>
    High
}
