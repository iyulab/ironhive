using Raggle.Abstractions.Json;
using System.ComponentModel;
using System.Reflection;

namespace Raggle.Abstractions.Tools;

public class FunctionTool
{
    public required Delegate Function { get; init; }
    public required string Name { get; init; }
    public string? Description { get; set; }

    public ObjectJsonSchema ToJsonSchema()
    {
        var parameters = Function.Method.GetParameters();
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();
        foreach (var prop in parameters)
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
            Description = Description,
            Properties = properties,
            Required = required.ToArray()
        };
    }

    public async Task<FunctionResult> InvokeAsync(FunctionArguments? args)
    {
        try
        {
            var result = Function.DynamicInvoke(PrepareArguments(args));
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
            else if (Function.Method.ReturnType == typeof(IAsyncEnumerable<>))
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
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return FunctionResult.Failed(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return FunctionResult.Failed(ex.Message);
        }
    }

    private object?[]? PrepareArguments(FunctionArguments? args)
    {
        var parameters = Function.Method.GetParameters();
        if (parameters.Length == 0) return null;
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramName = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");
            var paramType = param.ParameterType;
            
            if (args != null && args.TryGetValue(paramName, out var value))
            {
                arguments[i] = JsonObjectConverter.ConvertTo(paramType, value);
            }
            else if (param.HasDefaultValue)
            {
                arguments[i] = param.DefaultValue;
            }
            else if (param.IsOptional)
            {
                arguments[i] = null;
            }
            else
            {
                throw new ArgumentException($"Parameter '{paramName}' is required");
            }
        }

        return arguments;
    }

}
