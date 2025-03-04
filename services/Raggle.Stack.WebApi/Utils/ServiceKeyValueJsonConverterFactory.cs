using System.Text.Json.Serialization;
using System.Text.Json;

namespace Raggle.Stack.WebApi.Utils;

public class ServiceKeyValueJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(ServiceKeyValue<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(ServiceKeyValueJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
