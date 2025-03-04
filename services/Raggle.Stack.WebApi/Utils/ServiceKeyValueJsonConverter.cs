using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Raggle.Stack.WebApi.Utils;

public class ServiceKeyValueJsonConverter<T> : JsonConverter<ServiceKeyValue<T>>
{
    private const string KeyName = nameof(ServiceKeyValue<T>.ServiceKey);

    public override ServiceKeyValue<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var result = new ServiceKeyValue<T>();

        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var root = jsonDoc.RootElement;

            // "Key" 속성 이름을 NamingPolicy에 따라 변환하여 찾기
            string keyPropertyName = options.PropertyNamingPolicy?.ConvertName(KeyName) ?? KeyName;
            if (root.TryGetProperty(keyPropertyName, out JsonElement keyProp))
                result.ServiceKey = keyProp.GetString() ?? string.Empty;
            else
                throw new JsonException($"Missing property '{keyPropertyName}'");

            // 나머지 속성을 Value 객체로 역직렬화
            var valueProps = root.EnumerateObject().Where(p => !string.Equals(p.Name, keyPropertyName, StringComparison.OrdinalIgnoreCase));

            using var valueStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(valueStream))
            {
                writer.WriteStartObject();
                foreach (var prop in valueProps)
                {
                    prop.WriteTo(writer);
                }
                writer.WriteEndObject();
            }

            valueStream.Position = 0;
            result.Value = JsonSerializer.Deserialize<T>(valueStream, options)
                ?? throw new JsonException("Failed to deserialize the Value object");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, ServiceKeyValue<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // KeyName 속성 이름을 NamingPolicy에 따라 변환
        string keyName = options.PropertyNamingPolicy?.ConvertName(KeyName) ?? KeyName;
        writer.WriteString(keyName, value.ServiceKey);

        // Value 객체의 속성 이름을 NamingPolicy에 따라 변환하여 직렬화
        if (value.Value != null)
        {
            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                object? propertyValue = property.GetValue(value.Value);
                if (propertyValue == null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                    continue;

                string propName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                writer.WritePropertyName(propName);
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}

