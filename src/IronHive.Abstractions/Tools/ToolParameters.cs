using IronHive.Abstractions.Json;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// Represents the parameters of a tool.
/// </summary>
public class ToolParameters
{
    private readonly ParameterInfo[] _parameters;

    /// <summary>
    /// Gets the JSON schema for the tool parameters.
    /// </summary>
    public ObjectJsonSchema JsonSchema { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolParameters"/> class with the specified parameters.
    /// </summary>
    public ToolParameters(ParameterInfo[] parameters)
    {
        _parameters = parameters;
        JsonSchema = CreateJsonSchema(parameters);
    }

    /// <summary>
    /// Prepares the arguments for the tool invocation.
    /// </summary>
    /// <param name="args">Dictionary 또는 JSON 문자열 형식의 인자 객체</param>
    /// <returns>동적 호출에 사용할 object 배열</returns>
    public object?[]? BuildArguments(object? args)
    {
        if (_parameters.Length == 0 || args == null)
            return null;

        var dictionary = ParseObjectToDictionary(args);
        var arguments = new object?[_parameters.Length];

        for (int i = 0; i < _parameters.Length; i++)
        {
            var param = _parameters[i];
            var name = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");

            if (dictionary.TryGetValue(name, out var value))
            {
                // 인자가 존재하는 경우
                arguments[i] = value.ConvertTo(param.ParameterType);
            }
            else if (param.HasDefaultValue)
            {
                // 인자가 존재하지 않고, 기본값이 있는 경우
                arguments[i] = param.DefaultValue;
            }
            else if (param.IsOptional)
            {
                // 인자가 존재하지 않고, 선택적 인자인 경우
                arguments[i] = null;
            }
            else
            {
                // 인자가 존재하지 않고, 필수 인자인 경우
                throw new ArgumentException($"Parameter '{name}' is required");
            }
        }

        return arguments;
    }

    // 파라미터 정보를 기반으로 JSON 스키마를 생성합니다.
    private static ObjectJsonSchema CreateJsonSchema(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
        {
            return new ObjectJsonSchema();
        }

        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        foreach (var param in parameters)
        {
            if (string.IsNullOrEmpty(param.Name))
                continue;

            var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var schema = JsonSchemaFactory.CreateFrom(param.ParameterType, description);
            properties.Add(param.Name, schema);

            // 필수 속성인지 확인
            if (!param.IsOptional)
                required.Add(param.Name);
        }

        return new ObjectJsonSchema
        {
            Properties = properties,
            Required = required.Count != 0 ? required : null
        };
    }

    // object객체를 dictionary로 파싱합니다.
    private static IDictionary<string, object> ParseObjectToDictionary(object args)
    {
        if (args is IDictionary<string, object> dictionary)
        {
            return dictionary;
        }
        else if (args is string str)
        {
            return JsonSerializer.Deserialize<IDictionary<string, object>>(str)
                ?? throw new ArgumentException("Arguments must be a valid JSON object.");
        }
        else
        {
            var json = JsonSerializer.Serialize(args);
            return JsonSerializer.Deserialize<IDictionary<string, object>>(json) 
                ?? throw new ArgumentException("Arguments must be a valid JSON object.");
        }
    }
}
