namespace IronHive.Abstractions.Messages;

/// <summary>
/// Tool 제한 검증 결과를 나타냅니다.
/// </summary>
public sealed class ToolLimitValidationResult
{
    /// <summary>
    /// 검증 통과 여부
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// 실제 도구 수
    /// </summary>
    public int ActualCount { get; init; }

    /// <summary>
    /// 설정된 최대 도구 수
    /// </summary>
    public int MaxAllowed { get; init; }

    /// <summary>
    /// 초과된 도구 수 (초과하지 않으면 0)
    /// </summary>
    public int ExceededBy => IsValid ? 0 : ActualCount - MaxAllowed;

    /// <summary>
    /// 경고 또는 오류 메시지
    /// </summary>
    public string? Message { get; init; }
}

/// <summary>
/// Tool 수 제한 초과 시 발생하는 예외입니다.
/// </summary>
public sealed class ToolLimitExceededException : InvalidOperationException
{
    /// <summary>
    /// 실제 도구 수
    /// </summary>
    public int ActualCount { get; }

    /// <summary>
    /// 설정된 최대 도구 수
    /// </summary>
    public int MaxAllowed { get; }

    public ToolLimitExceededException(int actualCount, int maxAllowed)
        : base($"Tool limit exceeded: {actualCount} tools provided but maximum is {maxAllowed}. " +
               $"Reduce the number of tools or increase MaxTools setting.")
    {
        ActualCount = actualCount;
        MaxAllowed = maxAllowed;
    }
}
