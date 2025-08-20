namespace IronHive.Abstractions.Tools;

/// <summary>
/// LLM 도구(툴)를 정의하는 인터페이스입니다.
/// </summary>
public interface ITool
{
    /// <summary>
    /// 현재 도구를 식별하기 위한 유니크한 이름입니다.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// 도구의 이름입니다.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 도구의 설명입니다.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 도구의 매개변수입니다.
    /// </summary>
    object? Parameters { get; }

    /// <summary>
    /// 도구가 사용될 때 승인이 필요한지 여부를 나타냅니다.
    /// </summary>
    bool RequiresApproval { get; set; }

    /// <summary>
    /// 도구를 호출합니다.
    /// </summary>
    Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default);
}
