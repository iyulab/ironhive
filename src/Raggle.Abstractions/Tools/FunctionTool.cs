using Raggle.Abstractions.Schema;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace Raggle.Abstractions.Tools;

public class FunctionTool
{
    private readonly Delegate _function;
    public object? Target => _function.Target;
    public MethodInfo Method => _function.Method;
    public ParameterInfo[] Parameters => Method.GetParameters();
    public Type ReturnType => Method.ReturnType;

    public required string Name { get; set; }
    public string? Description { get; set; }

    public FunctionTool(Delegate function)
    {
        _function = function;
    }

    public ObjectJsonSchema GetParametersJsonSchema()
    {
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();
        foreach (var prop in Parameters)
        {
            if (string.IsNullOrEmpty(prop.Name))
                continue;

            var propType = prop.ParameterType;
            var propDescription = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var propSchema = JsonSchemaConverter.ConvertFromType(propType, propDescription);
            properties.Add(prop.Name, propSchema);

            if (!prop.IsOptional)
                required.Add(prop.Name);
        }

        return new ObjectJsonSchema
        {
            Properties = properties,
            Description = Description,
            Required = required.ToArray(),
        };
    }

    public async Task<FunctionResult> InvokeAsync(string? jsonArgs)
    {
        try
        {
            var args = jsonArgs != null
                ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonArgs)
                : null;

            var arguments = GetArguments(args);
            var result = _function.DynamicInvoke(arguments);

            if (result is Task task)
            {
                await task;
                result = task.GetType().GetProperty("Result")?.GetValue(task);
            }
            else if (result is ValueTask valueTask)
            {
                await valueTask;
                result = valueTask.GetType().GetProperty("Result")?.GetValue(valueTask);
            }
            else if (ReturnType == typeof(IAsyncEnumerable<>))
            {
                var enumerable = result as IAsyncEnumerable<object?>
                    ?? throw new InvalidOperationException("Expected IAsyncEnumerable but got null.");
                var list = new List<object?>();
                await foreach (var item in enumerable)
                {
                    list.Add(item);
                }
                result = list;
            }

            return FunctionResult.Success(result);
        }
        catch (JsonException)
        {
            return FunctionResult.Failed($"Invalid JSON format for arguments: {jsonArgs}");
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return FunctionResult.Failed(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return FunctionResult.Failed(ex.Message);
        }
    }

    private object?[]? GetArguments(Dictionary<string, JsonElement>? args)
    {
        if (Parameters.Length == 0) return null;
        var arguments = new object?[Parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
        {
            var param = Parameters[i];
            var paramName = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");
            var paramType = param.ParameterType;
            var isRequired = !param.IsOptional;
            
            if (args != null && args.TryGetValue(paramName, out var value))
            {
                arguments[i] = JsonSerializer.Deserialize(value.GetRawText(), paramType);
            }
            else if (param.HasDefaultValue)
            {
                arguments[i] = param.DefaultValue;
            }
            else if (isRequired)
            {
                throw new ArgumentException($"Required parameter '{paramName}' is missing.");
            }
            else
            {
                arguments[i] = null;
            }
        }

        return arguments;
    }

}
