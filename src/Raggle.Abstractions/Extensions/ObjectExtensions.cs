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
    public static bool TryGet<T>(this object obj, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null)
    {
        try
        {
            value = obj.Get<T>(options);
            return value != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to convert object to type {typeof(T).Name}: {ex.Message}");
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Converts the given object to the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to which the object should be converted.</typeparam>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The converted object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the object cannot be converted to type <typeparamref name="T"/>.</exception>
    public static T Get<T>(this object obj, JsonSerializerOptions? options = null)
    {
        if (obj is T t)
        {
            return t;
        }
        else if (obj is JsonDocument doc)
        {
            // Case 2: The object is a JsonDocument
            return doc.Deserialize<T>(options)
                ?? throw new InvalidOperationException($"Failed to convert object to type {typeof(T).Name}");
        }
        else if (obj is JsonElement el)
        {
            // Case 3: The object is a JsonElement
            return el.Deserialize<T>(options)
                ?? throw new InvalidOperationException($"Failed to convert object to type {typeof(T).Name}");
        }
        else
        {
            // Case 4: Attempt to serialize and deserialize the object to type T
            string jsonString = JsonSerializer.Serialize(obj, options);
            return JsonSerializer.Deserialize<T>(jsonString, options)
                ?? throw new InvalidOperationException($"Failed to convert object to type {typeof(T).Name}");
        }
    }
}
