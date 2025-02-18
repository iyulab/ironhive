namespace Raggle.Abstractions.Tools;

public class ToolResult
{
    public bool IsSuccess { get; set; }

    public object? Result { get; set; }

    public object? Error { get; set; }

    public ToolResult()
    { }

    public ToolResult(bool isSuccess, object? result, object? error)
    {
        IsSuccess = isSuccess;
        Result = result;
        Error = error;
    }

    public static ToolResult Success(object? result)
        => new(true, result, null);

    public static ToolResult Failed(object? error)
        => new(false, null, error);
}
