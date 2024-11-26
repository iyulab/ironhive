using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Raggle.Abstractions.Extensions;

public static class IDictionaryExtensions
{
    /// <summary>
    /// Tries to get the value associated with the specified key and converts it to the specified type.
    /// If the value is a <see cref="JsonDocument"/>, it attempts to deserialize it into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be converted.</typeparam>
    /// <param name="dictionary">The dictionary from which to retrieve the value.</param>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <param name="value">
    /// When this method returns, contains the converted value associated with the specified key,
    /// if the key is found and conversion succeeds; otherwise, the default value for the type.
    /// </param>
    /// <returns>
    /// true if the key was found and the value was successfully converted; otherwise, false
    /// </returns>
    public static bool TryGetValue<T>(
        this IDictionary<string, object> dictionary,
        string key,
        [NotNullWhen(true)] out T? value)
    {
        if (dictionary.TryGetValue(key, out object? obj))
        {
            // Case 1: The object is already of type T
            if (obj is T t)
            {
                value = t;
                return true;
            }

            // Case 2: The object is a JsonDocument
            if (obj is JsonDocument jsonDocument)
            {
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

            // Case 3: Attempt to serialize and deserialize the object to type T
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
