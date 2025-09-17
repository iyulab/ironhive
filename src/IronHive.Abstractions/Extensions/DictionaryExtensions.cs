using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    /// <summary>
    /// 문자열, 객체 딕셔너리에서 지정한 키의 값을 시도하여 가져옵니다.
    /// </summary>
    public static bool TryGetValue<TValue>(
        this IDictionary<string, object?> dic, 
        string key,
        [MaybeNullWhen(false)] out TValue value)
    {
        if (dic.TryGetValue(key, out var v) && v.TryConvertTo(out value))
            return true;

        value = default;
        return false;
    }
}
