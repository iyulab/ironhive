using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    /// <summary>
    /// <paramref name="key"/>로 등록된 값을 가져옵니다. <paramref name="key"/>가 비어 있으면 단일
    /// 등록된 값이 자동 선택되고, 둘 이상 등록돼 있으면 명시를 요구하는 예외가 발생합니다.
    /// 에러 메시지에는 <typeparamref name="TValue"/>의 타입 이름이 항목 이름으로 쓰입니다.
    /// </summary>
    public static TValue GetOrFirstValue<TValue>(
        this IReadOnlyDictionary<string, TValue> source,
        string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            var typeName = typeof(TValue).Name;
            if (source.Count == 0)
                throw new InvalidOperationException($"No {typeName} is registered.");
            if (source.Count > 1)
                throw new InvalidOperationException(
                    $"Multiple {typeName} entries are registered ({string.Join(", ", source.Keys)}). " +
                    "Specify a provider.");
            return source.Values.First();
        }

        if (!source.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"{typeof(TValue).Name} '{key}' is not registered.");
        return value;
    }
}
