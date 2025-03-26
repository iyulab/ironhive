using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace IronHive.Core.Tools;

public class FunctionTool : ITool
{
    private readonly Delegate _function;

    public required string Name { get; set; }
    public string? Description { get; set; }
    public ObjectJsonSchema? Parameters { get; set; }

    public FunctionTool(Delegate function)
    {
        _function = function;
        Parameters = GetParametersJsonSchema(_function.Method.GetParameters());
    }

    public async Task<ToolResult> InvokeAsync(object? args)
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

    private object?[]? PrepareArguments(object? args)
    {
        var parameters = _function.Method.GetParameters();
        if (parameters.Length == 0) return null;

        IDictionary<string, object> dict;
        if (args is string str)
        {
            dict = JsonSerializer.Deserialize<IDictionary<string, object>>(str)
                ?? throw new ArgumentException("Arguments must be a valid JSON object.");
        }
        else if (args is IDictionary<string, object> dictionary)
        {
            dict = dictionary;
        }
        else
        {
            throw new ArgumentException("Arguments must be a valid JSON object.");
        }
        
        var arguments = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var name = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");
            var type = param.ParameterType;

            if (dict.TryGetValue(name, out var value))
            {
                arguments[i] = value.ConvertTo(type);
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
                throw new ArgumentException($"Parameter '{name}' is required");
            }
        }

        return arguments;
    }

    private static ObjectJsonSchema? GetParametersJsonSchema(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
        {
            return null;
        }
        else
        {
            var props = new Dictionary<string, JsonSchema>();
            var required = new List<string>();

            foreach (var pram in parameters)
            {
                if (string.IsNullOrEmpty(pram.Name))
                    continue;

                var type = pram.ParameterType;
                var name = pram.Name;
                var description = pram.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var schema = JsonSchemaConverter.ConvertFrom(type, description);
                props.Add(name, schema);

                if (!pram.IsOptional)
                    required.Add(name);
            }

            return new ObjectJsonSchema
            {
                Properties = props,
                Required = required.Count != 0 ? required : null
            };
        }
    }

}
