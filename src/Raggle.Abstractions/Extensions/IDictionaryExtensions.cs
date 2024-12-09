using Raggle.Abstractions.Json;
using System.Diagnostics.CodeAnalysis;

namespace Raggle.Abstractions.Extensions;

public static class IDictionaryExtensions
{
    public static bool TryGetValue<T>(
        this IDictionary<string, object> dictionary, 
        string key,
        [NotNullWhen(true)] out T value)
    {
        if (dictionary.TryGetValue(key, out object? obj))
        {
            var json = JsonObjectConverter.ConvertTo<T>(obj);
            if (json != null)
            {
                value = json;
                return true;
            }
        }

        value = default!;
        return false;
    }
}
