using System.Text.Json;

namespace IronHive.Abstractions.ChatCompletion.Tools;

public class FunctionResult
{
    public bool IsSuccess { get; set; }

    public object? Data { get; set; }

    public FunctionResult()
    { }

    public FunctionResult(bool isSuccess, object? data)
    {
        IsSuccess = isSuccess;
        Data = data;
    }

    public static FunctionResult Success(object? result)
        => new(true, result);

    public static FunctionResult Failed(object? error)
        => new(false, error);
}
