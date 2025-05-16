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
    /// if success, this will be the result of the tool result of string type,
    /// else, this will be the error message.
    /// </summary>
    public string? Data { get; set; }

    public ToolResult()
    { }

    public ToolResult(bool isSuccess, string? data)
    {
        IsSuccess = isSuccess;
        Data = data;
    }

    public static ToolResult Success(string? result)
        => new(true, result);

    public static ToolResult Failure(string? error)
        => new(false, error);

    public static ToolResult ToolNotFound(string toolName)
        => Failure($"Tool [{toolName}] not found. Please check the tool name for any typos or consult the documentation for available tools.");

    public static ToolResult InvocationRejected()
        => Failure("User denied this tool call request. Please check with the user for reasons.");

    public static ToolResult ExcessiveResult()
        => Failure("The result contains too much information. Please change the parameters or specify additional filters to obtain a smaller result set.");
}
