using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Raggle.Abstractions.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Tries to get the value of the specified type <typeparamref name="T"/> from the given object.
    /// </summary>
    /// <typeparam name="T">The type to which the object should be converted.</typeparam>
    /// <param name="obj">The object to convert.</param>
    /// <param name="value">When this method returns, contains the converted value if the conversion succeeded, or the default value of <typeparamref name="T"/> if the conversion failed.</param>
    /// <returns><c>true</c> if the object was successfully converted to <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
    public static bool TryGet<T>(this object obj, [NotNullWhen(true)] out T? value)
    {
        if (obj is T t)
        {
            // Case 1: The object is already of type T
            value = t;
            return true;
        }
        else if (obj is JsonDocument jsonDocument)
        {
            // Case 2: The object is a JsonDocument
            try
            {
                value = jsonDocument.Deserialize<T>();
                return value is not null;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Failed to convert JsonDocument to type {typeof(T).Name}: {ex.Message}");
            }
        }
        else if (obj is JsonElement el)
        {
            // Case 3: The object is a JsonElement
            try
            {
                value = el.Deserialize<T>();
                return value is not null;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Failed to convert JsonElement to type {typeof(T).Name}: {ex.Message}");
            }
        }
        else
        {
            // Case 4: Attempt to serialize and deserialize the object to type T
            try
            {
                string jsonString = JsonSerializer.Serialize(obj);
                value = JsonSerializer.Deserialize<T>(jsonString);
                return value is not null;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Failed to convert object to type {typeof(T).Name}: {ex.Message}");
            }
        }

        value = default;
        return false;
    }
}
