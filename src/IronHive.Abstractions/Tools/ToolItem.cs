namespace IronHive.Abstractions.Tools;

/// <summary>
/// LLM이 사용할 수 있는 도구 항목을 나타냅니다.
/// 각 도구 항목은 고유한 이름(Name)과 선택적 옵션(Options)으로 정의됩니다.
/// </summary>
public class ToolItem
{
    /// <summary>
    /// 도구의 고유 식별자입니다.
    /// LLM이 해당 도구를 호출할 때 이 이름을 사용합니다.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 도구 실행에 필요한 옵션 값입니다.
    /// - 구조는 도구별로 다를 수 있으며, 없을 경우 <c>null</c>일 수 있습니다.
    /// </summary>
    public object? Options { get; set; }
}
