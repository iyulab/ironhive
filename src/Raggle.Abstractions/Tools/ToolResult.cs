namespace Raggle.Abstractions.Tools;

public class ToolResult
{
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public object? Error { get; set; }

    public static ToolResult Success(object? result)
    {
        return new ToolResult
        {
            IsSuccess = true,
            Result = result
        };
    }

    public static ToolResult Failed(object? error)
    {
        return new ToolResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}
