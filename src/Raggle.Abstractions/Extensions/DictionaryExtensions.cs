using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, object> dic, 
        string key, 
        [MaybeNullWhen(false)] out T value)
    {
        if (dic.TryGetValue(key, out object? obj))
        {
            value = obj.ConvertTo<T>();
            return value != null;
        }
        else
        {
            value = default;
            return false;
        }
    }


}
