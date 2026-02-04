namespace IronHive.Abstractions.Messages;

/// <summary>
/// Tool 수 제한 초과 시 동작을 정의합니다.
/// </summary>
public enum ToolLimitBehavior
{
    /// <summary>
    /// 제한을 무시하고 모든 도구를 포함합니다.
    /// </summary>
    Ignore,

    /// <summary>
    /// 경고 로그를 남기고 모든 도구를 포함합니다.
    /// </summary>
    Warn,

    /// <summary>
    /// 예외를 발생시킵니다.
    /// </summary>
    Throw,

    /// <summary>
    /// MaxTools 개수까지만 도구를 포함하고 나머지는 무시합니다.
    /// </summary>
    Truncate
}
