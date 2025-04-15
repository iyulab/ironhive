using IronHive.Abstractions.Json;
using System.ComponentModel;

namespace System.Reflection;

public static class MethodInfoExtensions
{
    /// <summary>
    /// Get the input JSON schema for the method.
    /// </summary>
    public static ObjectJsonSchema? GetInputJsonSchema(this MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return null;
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
}
