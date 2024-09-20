namespace Raggle.Abstractions.Tools;

public class FunctionResult
{
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public static FunctionResult Success(object? result)
    {
        return new FunctionResult
        {
            IsSuccess = true,
            Result = result
        };
    }

    public static FunctionResult Failed(string? errorMessage)
    {
        return new FunctionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
