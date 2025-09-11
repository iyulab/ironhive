namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 작업 스텝의 실행 결과
/// </summary>
public sealed class TaskStepResult
{
    public bool IsError { get; init; }
    
    public string? Message { get; init; }

    public Exception? Exception { get; init; }

    public static TaskStepResult Success(string? message = null)
        => new() { IsError = false, Message = message };

    public static TaskStepResult Fail(Exception exception)
        => new() { IsError = true, Exception = exception, Message = exception.Message };
}

/// <summary>
/// 조건 스텝의 실행 결과
/// </summary>
public sealed class ConditionStepResult
{
    public required string Key { get; init; }

    public string? Message { get; init; }

    public static ConditionStepResult Select(string key, string? message = null)
        => new() { Key = key, Message = message };
}
