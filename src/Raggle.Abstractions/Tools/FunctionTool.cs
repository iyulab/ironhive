using System.Reflection;

namespace Raggle.Abstractions.Tools;

public class FunctionTool
{
    private readonly Delegate _function;
    public object? Target => _function.Target;
    public MethodInfo Method => _function.Method;
    public ParameterInfo[] Parameters => _function.Method.GetParameters();
    public Type ReturnType => _function.Method.ReturnType;

    public required string Name { get; set; }
    public string? Description { get; set; }
    public record FunctionResult(bool IsSuccess, object? Result, string? ErrorMessage);

    public FunctionTool(Delegate function)
    {
        _function = function;
    }

    public async Task<FunctionResult> InvokeAsync(IDictionary<string, object?>? args)
    {
        try
        {
            var arguments = GetArguments(args);
            var result = _function.DynamicInvoke(arguments);
            if (result is Task task)
            {
                await task;
                var resultProperty = task.GetType().GetProperty("Result");
                return new FunctionResult(true, resultProperty?.GetValue(task), null);
            }
            return new FunctionResult(true, result, null);
        }
        catch (Exception ex)
        {
            return new FunctionResult(false, null, ex.Message);
        }
    }

    private object?[] GetArguments(IDictionary<string, object?>? args)
    {
        var arguments = new object?[Parameters.Length];
        if (args == null || args.Count == 0)
        {
            return arguments;
        }

        for (int i = 0; i < Parameters.Length; i++)
        {
            var param = Parameters[i];
            var paramName = param.Name!;
            var paramType = param.ParameterType;
            var isRequired = !param.IsOptional;

            if (args.TryGetValue(paramName, out var value))
            {
                arguments[i] = value;
                continue;
            }
            if (isRequired)
            {
                throw new ArgumentException($"Required parameter '{param.Name}' is missing.");
            }
        }
        return arguments;
    }

}


