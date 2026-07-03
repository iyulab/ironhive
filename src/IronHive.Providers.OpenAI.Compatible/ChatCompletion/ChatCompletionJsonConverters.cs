using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>Collapses a single-text-part user message to a plain JSON string, matching the shorthand
/// most OpenAI-compatible servers accept alongside the full content-part array form.</summary>
internal sealed class ChatMessageContentJsonConverter : JsonConverter<ICollection<ChatMessageContent>>
{
    public override ICollection<ChatMessageContent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString() ?? string.Empty;
            return [new TextChatMessageContent { Text = text }];
        }
        return JsonSerializer.Deserialize<ICollection<ChatMessageContent>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, ICollection<ChatMessageContent> value, JsonSerializerOptions options)
    {
        if (value.Count == 1 && value.First() is TextChatMessageContent tc)
        {
            writer.WriteStringValue(tc.Text);
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}

/// <summary>
/// For <see cref="ChatCompletionPayloadBase"/> subtypes, deep-merges <c>ExtraBody</c> into the serialized JSON
/// and, on deserialization, collects properties the typed model doesn't recognize back into <c>ExtraBody</c> —
/// this is what lets <see cref="ChatCompletionMessageGenerator"/> read vendor fields like
/// <c>choices[0].message.reasoning_content</c> that no OpenAI-compatible server's shape is guaranteed to match
/// exactly.
/// </summary>
internal sealed class ExtraBodyJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(ChatCompletionPayloadBase).IsAssignableFrom(typeToConvert) && !typeToConvert.IsAbstract;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ExtraBodyJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class ExtraBodyJsonConverter<T> : JsonConverter<T> where T : ChatCompletionPayloadBase
{
    private JsonSerializerOptions? _innerOptions;

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var inner = EnsureInnerOptions(options);

        var obj = JsonSerializer.SerializeToNode(value, inner)!.AsObject();

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

        var result = element.Deserialize<T>(inner)!;

        var extras = ExtractUnknownProperties(element, typeof(T), inner);
        if (extras is { Count: > 0 })
            result.ExtraBody = extras;

        return result;
    }

    // Recursion guard: strip the factory from a private copy of the options before delegating to STJ.
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

    private static JsonObject? ExtractUnknownProperties(
        JsonElement element, Type targetType, JsonSerializerOptions options)
    {
        var knownProps = GetPropertyLookup(targetType, options);
        JsonObject? extras = null;

        foreach (var prop in element.EnumerateObject())
        {
            if (knownProps.TryGetValue(prop.Name, out var propType))
            {
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    var nested = ExtractUnknownProperties(prop.Value, propType, options);
                    if (nested is { Count: > 0 })
                    {
                        extras ??= new JsonObject();
                        extras[prop.Name] = nested;
                    }
                }
                else if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    JsonArray? arrayExtras = null;
                    Type? elementType = propType.IsArray ? propType.GetElementType()
                        : (propType.IsGenericType ? propType.GetGenericArguments()[0] : null);

                    if (elementType != null)
                    {
                        var index = 0;
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                var nested = ExtractUnknownProperties(item, elementType, options);
                                if (nested is { Count: > 0 })
                                {
                                    arrayExtras ??= new JsonArray();
                                    // Keep the extras array's index in sync with the source array.
                                    while (arrayExtras.Count < index)
                                        arrayExtras.Add(new JsonObject());
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
