using Raggle.Abstractions.ChatCompletion.Tools;
using Raggle.Abstractions.Json;
using System.ComponentModel;
using System.Reflection;

namespace Raggle.Core.ChatCompletion;

public class FunctionTool : ITool
{
    private readonly Delegate _function;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IDictionary<string, JsonSchema>? Parameters { get; set; }
    public IEnumerable<string>? Required { get; set; }

    public FunctionTool(
        Delegate function,
        string? name = null,
        string? description = null)
    {
        _function = function;
        Name = name ?? function.Method.Name;
        Description = description;
        var schema = GetJsonSchema();
        Parameters = schema.Properties;
        Required = schema.Required;
    }

    public async Task<ToolResult> InvokeAsync(ToolArguments? args)
    {
        try
        {
            var result = _function.DynamicInvoke(PrepareArguments(args));
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
            else if (_function.Method.ReturnType == typeof(IAsyncEnumerable<>))
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

            return ToolResult.Success(result);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return ToolResult.Failed(ex.InnerException.Message);
        }
        catch (Exception ex)
        {
            return ToolResult.Failed(ex.Message);
        }
    }

    private object?[]? PrepareArguments(ToolArguments? args)
    {
        var parameters = _function.Method.GetParameters();
        if (parameters.Length == 0) return null;
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramName = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");
            var paramType = param.ParameterType;

            if (args != null && args.TryGetValue(paramName, out var value))
            {
                arguments[i] = value.ConvertTo(paramType);
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

    private ObjectJsonSchema GetJsonSchema()
    {
        var parameters = _function.Method.GetParameters();
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        foreach (var prop in parameters)
        {
            if (string.IsNullOrEmpty(prop.Name))
                continue;

            var propType = prop.ParameterType;
            var propDescription = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var propSchema = JsonSchemaConverter.ConvertFrom(propType, propDescription);
            properties.Add(prop.Name, propSchema);

            if (!prop.IsOptional)
                required.Add(prop.Name);
        }

        return new ObjectJsonSchema
        {
            Properties = properties,
            Required = required.Count != 0 ? required.ToArray() : null
        };
    }

}
