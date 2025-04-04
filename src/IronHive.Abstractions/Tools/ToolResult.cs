namespace IronHive.Abstractions.Tools;

/// <summary>
/// Represents the result of a tool execution.
/// </summary>
public class ToolResult
{
    /// <summary>
    /// Indicates whether the tool execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// The data returned from the tool execution.
    /// if success, this will be the result of the tool.
    /// else, this will be the error message.
    /// </summary>
    public object? Data { get; set; }

    public ToolResult()
    { }

    public ToolResult(bool isSuccess, object? data)
    {
        IsSuccess = isSuccess;
        Data = data;
    }

    public static ToolResult Success(object? result)
        => new(true, result);

    public static ToolResult Failed(object? error)
        => new(false, error);
}
