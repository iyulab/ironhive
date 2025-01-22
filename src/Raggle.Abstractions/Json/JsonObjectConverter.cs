using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Raggle.Abstractions.Json;

public static class JsonObjectConverter
{
    public static bool TryConvertTo<T>(object? obj, [NotNullWhen(true)] out T value)
    {
        var json = ConvertTo<T>(obj);
        if (json != null)
        {
            value = json;
            return true;
        }

        value = default!;
        return false;
    }

    public static T? ConvertTo<T>(object? obj)
    {
        try
        {
            if (obj is T t)
            {
                return t;
            }
            else if (obj is JsonDocument doc)
            {
                // Case 2: The object is a JsonDocument
                return doc.Deserialize<T>(RaggleOptions.JsonOptions);
            }
            else if (obj is JsonElement el)
            {
                // Case 3: The object is a JsonElement
                return el.Deserialize<T>(RaggleOptions.JsonOptions);
            }
            else if (obj is JsonObject jo)
            {
                // Case 4: The object is a JsonObject
                return jo.Deserialize<T>(RaggleOptions.JsonOptions);
            }
            else if (obj is JsonNode node)
            {
                // Case 5: The object is a JsonNode
                return node.Deserialize<T>(RaggleOptions.JsonOptions);
            }
            else
            {
                // Case 6: Attempt to serialize and deserialize the object to type T
                string jsonString = JsonSerializer.Serialize(obj, RaggleOptions.JsonOptions);
                return JsonSerializer.Deserialize<T>(jsonString, RaggleOptions.JsonOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return default;
        }
    }

    public static object? ConvertTo(Type targetType, object? obj)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(targetType);

            if (obj == null)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            if (targetType.IsAssignableFrom(obj.GetType()))
            {
                return obj;
            }

            if (obj is JsonDocument doc)
            {
                // Case 2: The object is a JsonDocument
                return doc.Deserialize(targetType, RaggleOptions.JsonOptions);
            }
            else if (obj is JsonElement el)
            {
                // Case 3: The object is a JsonElement
                return JsonSerializer.Deserialize(el.GetRawText(), targetType, RaggleOptions.JsonOptions);
            }
            else if (obj is JsonObject jo)
            {
                // Case 4: The object is a JsonObject
                return jo.Deserialize(targetType, RaggleOptions.JsonOptions);
            }
            else if (obj is JsonNode node)
            {
                // Case 5: The object is a JsonNode
                return node.Deserialize(targetType, RaggleOptions.JsonOptions);
            }
            else
            {
                // Case 6: Attempt to serialize and deserialize the object to the target type
                string jsonString = JsonSerializer.Serialize(obj, RaggleOptions.JsonOptions);
                return JsonSerializer.Deserialize(jsonString, targetType, RaggleOptions.JsonOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }
}
