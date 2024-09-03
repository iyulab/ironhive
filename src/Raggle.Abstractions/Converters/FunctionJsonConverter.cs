using Raggle.Abstractions.Models;
using System.ComponentModel;
using System.Reflection;

namespace Raggle.Abstractions.Converters;

public static class FunctionJsonConverter
{
    public static JsonSchema Convert(MethodInfo methodInfo)
    {
        var name = methodInfo.Name;
        var description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
        var parameters = methodInfo.GetParameters();
        var requiredParameters = parameters.Where(parameter => parameter.HasDefaultValue).Select(parameter => parameter.Name);

        parameters = parameters.OrderBy(parameter => parameter.Position).ToArray();
        object[] schema = parameters.Select(parameter =>
        {
            var parameterType = parameter.ParameterType;
            var parameterName = parameter.Name;
            var parameterDescription = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var parameterRequired = requiredParameters.Contains(parameterName);

            return new
            {
                Name = parameterName,
                Type = parameterType.Name,
                Description = parameterDescription,
            };
        }).ToArray();

        return new JsonSchema
        {
            Name = name,
            Description = description,
            Parameters = schema
        };
    }
}
