using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.JsonConverters;

/// <summary>
/// <see cref="OpenAIPayloadBase"/>를 상속하는 타입에 대해
/// ExtraBody를 deep merge/split 처리하는 컨버터 팩토리입니다.
/// </summary>
public class ExtraBodyJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // OpenAIPayloadBase를 상속/구현한 concrete 타입만
        if (!typeof(OpenAIPayloadBase).IsAssignableFrom(typeToConvert) || typeToConvert.IsAbstract)
            return false;

        // 폴리모픽 스트리밍 이벤트 계열은 제외 (STJ 다형성과 충돌 방지)
        if (IsPolymorphicFamily(typeToConvert))
            return false;

        return true;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ExtraBodyJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static bool IsPolymorphicFamily(Type typeToConvert)
    {
        // 자기 자신 + 상속 체인 중 어디든 JsonPolymorphic / JsonDerivedType 있으면 폴리모픽 패밀리로 간주
        for (var cur = typeToConvert; cur != null && cur != typeof(object); cur = cur.BaseType)
        {
            if (Attribute.IsDefined(cur, typeof(JsonPolymorphicAttribute), inherit: false))
                return true;

            if (cur.GetCustomAttributes(typeof(JsonDerivedTypeAttribute), inherit: false).Length > 0)
                return true;
        }

        return false;
    }
}

/// <summary>
/// ExtraBody를 직렬화 시 루트에 deep merge하고,
/// 역직렬화 시 미매핑 속성을 ExtraBody로 수집하는 컨버터입니다.
/// inner options(팩토리 제거된 복사본)를 통해 STJ에 위임하여 재귀를 방지합니다.
/// </summary>
internal sealed class ExtraBodyJsonConverter<T> : JsonConverter<T> where T : OpenAIPayloadBase
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
                // 1. 이미 알려진 속성이 '객체(Object)' 타입인 경우 내부를 더 탐색합니다.
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    var nested = ExtractUnknownProperties(prop.Value, propType, options);
                    if (nested is { Count: > 0 })
                    {
                        extras ??= new JsonObject();
                        // 하위 계층 구조(예: choices -> message)를 그대로 유지하며 병합합니다.
                        extras[prop.Name] = nested;
                    }
                }
                // 2. 알려진 속성이 '배열(Array)'인 경우 (choices 가여기에 해당)
                else if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    JsonArray? arrayExtras = null;

                    // 배열 내부 요소가 객체라면, 각 요소 안의 미매핑 속성을 검사합니다.
                    // 이 때 choices[0] 의 타입(예: Choice)을 알아내기 위해 제네릭 아규먼트나 엘리먼트 타입을 추적합니다.
                    Type? elementType = propType.IsArray ? propType.GetElementType()
                        : (propType.IsGenericType ? propType.GetGenericArguments()[0] : null);

                    if (elementType != null)
                    {
                        int index = 0;
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                var nested = ExtractUnknownProperties(item, elementType, options);
                                if (nested is { Count: > 0 })
                                {
                                    arrayExtras ??= new JsonArray();

                                    // 원본 배열과 인덱스 싱크를 맞추기 위해 
                                    // 앞선 요소들이 빈 값이라면 빈 객체{}로 채워줍니다.
                                    while (arrayExtras.Count < index)
                                    {
                                        arrayExtras.Add(new JsonObject());
                                    }

                                    arrayExtras.Add(nested);
                                }
                            }
                            index++;
                        }
                    }

                    if (arrayExtras is { Count: > 0 })
                    {
                        extras ??= new JsonObject();
                        extras[prop.Name] = arrayExtras;
                    }
                }
            }
            else
            {
                // 3. 미매핑 속성 발견 시 기존과 동일하게 수집
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
