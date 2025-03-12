using System.Text.Json;

namespace Raggle.Abstractions.ChatCompletion.Tools;

public class ToolResult
{
    public bool IsSuccess { get; set; }

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

    public override string ToString()
    {
        if (IsSuccess)
            return JsonSerializer.Serialize(Data);
        else
            return $"Failed with Error: {Data}";
    }
}
