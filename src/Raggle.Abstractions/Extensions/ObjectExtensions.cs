using Raggle.Abstractions.Json;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace System;

public static class ObjectExtensions
{
    /// <summary>
    /// Try to convert the object to the target type. 
    /// If the conversion fails, the null value is returned.
    /// </summary>
    public static bool TryConvertTo<T>(this object? obj, [MaybeNullWhen(false)] out T value)
    {
        value = obj.ConvertTo<T>();
        return value != null;
    }

    /// <summary>
    /// Convert the object to the target type. If the conversion fails, the default value is returned.
    /// </summary>
    public static T? ConvertTo<T>(this object? obj)
    {
        return (T?)obj.ConvertTo(typeof(T));
    }

    /// <summary>
    /// Convert the object to the target type. If the conversion fails, the default value is returned.
    /// </summary>
    public static object? ConvertTo(this object? obj, Type target)
    {
        try
        {
            if (obj == null)
            {
                // Case 1: The object is null
                return null;
            }
            else if (target.IsAssignableFrom(obj.GetType()))
            {
                // Case 2: The object is already the target type
                return obj;
            }
            else if (obj is JsonDocument doc)
            {
                // Case 3: The object is a JsonDocument
                return doc.Deserialize(target, JsonDefaultOptions.Options);
            }
            else if (obj is JsonElement el)
            {
                // Case 4: The object is a JsonElement
                return JsonSerializer.Deserialize(el.GetRawText(), target, JsonDefaultOptions.Options);
            }
            else if (obj is JsonObject jo)
            {
                // Case 5: The object is a JsonObject
                return jo.Deserialize(target, JsonDefaultOptions.Options);
            }
            else if (obj is JsonNode node)
            {
                // Case 6: The object is a JsonNode
                return node.Deserialize(target, JsonDefaultOptions.Options);
            }
            else
            {
                // Case 7: Attempt to serialize and deserialize the object to the target type
                string jsonStr = JsonSerializer.Serialize(obj, JsonDefaultOptions.Options);
                return JsonSerializer.Deserialize(jsonStr, target, JsonDefaultOptions.Options);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// perform a deep copy of the object
    /// </summary>
    public static T DeepCopy<T>(this T obj)
    {
        // if obj is null, a value type, or a string, return the object
        if (obj == null || obj.GetType().IsValueType || obj is string)
        {
            return obj;
        }

        try
        {
            var json = JsonSerializer.Serialize(obj, JsonDefaultOptions.CopyOptions);
            var clone = JsonSerializer.Deserialize<T>(json, JsonDefaultOptions.CopyOptions)
                ?? throw new SerializationException("Failed to deserialize the object.");
            return clone;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }
}
