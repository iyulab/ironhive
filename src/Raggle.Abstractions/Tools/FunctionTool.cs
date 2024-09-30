using Raggle.Abstractions.Converters;
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
    public IDictionary<string, object> Properties { get; set; }
    public string[] Required { get; set; }

    public FunctionTool(Delegate function)
    {
        _function = function;
        var jsonSchema = GetSchemaInfo(Parameters);
        Properties = jsonSchema.Item1;
        Required = jsonSchema.Item2;
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
                result = task.GetType().GetProperty("Result")?.GetValue(task);
            }
            return FunctionResult.Success(result);
        }
        catch (Exception ex)
        {
            return FunctionResult.Failed(ex.Message);
        }
    }

    private object?[]? GetArguments(IDictionary<string, object?>? args)
    {
        if (Parameters.Length == 0)
        {
            return null;
        }

        var arguments = new object?[Parameters.Length];
        var hasArgs = args != null && args.Count > 0;

        for (int i = 0; i < Parameters.Length; i++)
        {
            var param = Parameters[i];
            var paramName = param.Name;
            var paramType = param.ParameterType;
            var isRequired = !param.IsOptional;
            
            if (hasArgs && args!.TryGetValue(paramName!, out var value))
            {
                if (value is JsonElement el)
                {
                    arguments[i] = JsonSerializer.Deserialize(el.GetRawText(), paramType);
                }
                else
                {
                    arguments[i] = value;
                }
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

    private static (IDictionary<string, object>, string[]) GetSchemaInfo(ParameterInfo[] parameters)
    {
        var properties = new Dictionary<string, object>();
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

        return (properties, required.ToArray());
    }

}
