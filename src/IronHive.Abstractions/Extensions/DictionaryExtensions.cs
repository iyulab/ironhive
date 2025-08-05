using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    /// <summary>
    /// 딕셔너리에서 지정한 키에 해당하는 값을 지정한 형식으로 변환하여 가져옵니다.
    /// 변환에 성공하면 true를 반환하고, 실패하거나 값이 없으면 false를 반환합니다.
    /// </summary>
    /// <param name="key">검색할 키</param>
    /// <param name="value">성공 시 변환된 값, 실패 시 기본값</param>
    /// <returns>값을 성공적으로 변환한 경우 true, 그렇지 않으면 false</returns>
    public static bool TryGetValue<T>(
        this IDictionary<string, object?> dic, 
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
