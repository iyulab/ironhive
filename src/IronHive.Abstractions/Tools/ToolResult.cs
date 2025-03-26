namespace IronHive.Abstractions.Tools;

public class ToolResult
{
    public bool IsSuccess { get; set; } = false;

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
