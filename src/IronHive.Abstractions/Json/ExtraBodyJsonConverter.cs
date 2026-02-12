using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// <see cref="JsonExtensibleBase"/>를 상속하는 타입에 대해
/// ExtraBody를 deep merge/split 처리하는 컨버터 팩토리입니다.
/// </summary>
public class ExtraBodyJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(JsonExtensibleBase).IsAssignableFrom(typeToConvert)
        && !typeToConvert.IsAbstract;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ExtraBodyJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// ExtraBody를 직렬화 시 루트에 deep merge하고,
/// 역직렬화 시 미매핑 속성을 ExtraBody로 수집하는 컨버터입니다.
/// inner options(팩토리 제거된 복사본)를 통해 STJ에 위임하여 재귀를 방지합니다.
/// </summary>
internal class ExtraBodyJsonConverter<T> : JsonConverter<T> where T : JsonExtensibleBase
{
    private JsonSerializerOptions? _innerOptions;

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var inner = EnsureInnerOptions(options);

        // STJ로 직렬화 → JsonObject
        var obj = JsonSerializer.SerializeToNode(value, inner)!.AsObject();

        // ExtraBody deep merge
        if (value.ExtraBody is { Count: > 0 })
            DeepMerge(obj, value.ExtraBody);

        obj.WriteTo(writer);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            return null;

        var inner = EnsureInnerOptions(options);
        var element = JsonElement.ParseValue(ref reader);

        // STJ로 역직렬화 (known 프로퍼티 처리)
        var result = element.Deserialize<T>(inner)!;

        // deep split: 미매핑 속성을 ExtraBody로 추출
        var extras = ExtractUnknownProperties(element, typeof(T), inner);
        if (extras is { Count: > 0 })
            result.ExtraBody = extras;

        return result;
    }

    // --- Inner options (재귀 방지용, 팩토리 제거된 복사본) ---

    private JsonSerializerOptions EnsureInnerOptions(JsonSerializerOptions options)
    {
        if (_innerOptions != null)
            return _innerOptions;

        var inner = new JsonSerializerOptions(options);
        for (var i = inner.Converters.Count - 1; i >= 0; i--)
        {
            if (inner.Converters[i] is ExtraBodyJsonConverterFactory)
                inner.Converters.RemoveAt(i);
        }
        _innerOptions = inner;
        return _innerOptions;
    }

    // --- Write helpers ---

    private static void DeepMerge(JsonObject target, JsonObject source)
    {
        foreach (var kvp in source)
        {
            if (target.TryGetPropertyValue(kvp.Key, out var existing)
                && existing is JsonObject existingObj
                && kvp.Value is JsonObject sourceObj)
            {
                DeepMerge(existingObj, sourceObj.DeepClone().AsObject());
            }
            else
            {
                target[kvp.Key] = kvp.Value?.DeepClone();
            }
        }
    }

    // --- Read helpers ---

    private static JsonObject? ExtractUnknownProperties(
        JsonElement element, Type targetType, JsonSerializerOptions options)
    {
        var knownProps = GetPropertyLookup(targetType, options);
        JsonObject? extras = null;

        foreach (var prop in element.EnumerateObject())
        {
            if (knownProps.TryGetValue(prop.Name, out var propType))
            {
                // known 프로퍼티 내부에서 재귀적으로 미매핑 속성 추출
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    var nested = ExtractUnknownProperties(prop.Value, propType, options);
                    if (nested is { Count: > 0 })
                    {
                        extras ??= new JsonObject();
                        extras[prop.Name] = nested;
                    }
                }
            }
            else
            {
                extras ??= new JsonObject();
                extras[prop.Name] = JsonNode.Parse(prop.Value.GetRawText());
            }
        }

        return extras;
    }

    private static Dictionary<string, Type> GetPropertyLookup(Type type, JsonSerializerOptions options)
    {
        var comparer = options.PropertyNameCaseInsensitive
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var result = new Dictionary<string, Type>(comparer);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition == JsonIgnoreCondition.Always)
                continue;

            var jsonName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? options.PropertyNamingPolicy?.ConvertName(prop.Name)
                ?? prop.Name;

            result[jsonName] = prop.PropertyType;
        }

        return result;
    }
}
