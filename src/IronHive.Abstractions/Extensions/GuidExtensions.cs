namespace System;

public static class GuidExtensions
{
    /// <summary>
    /// Guid 값을 짧은 문자열로 변환합니다.
    /// 선택적으로 하이픈을 제거하고, 지정된 길이만큼 자를 수 있습니다.
    /// </summary>
    /// <param name="length">반환할 문자열의 최대 길이 (null이면 전체)</param>
    /// <param name="withoutHyphens">하이픈(-)을 제거할지 여부 (기본값: true)</param>
    /// <returns>짧게 변환된 Guid 문자열</returns>
    public static string ToShort(
        this Guid guid, 
        int? length = null, 
        bool withoutHyphens = true)
    {
        var str = guid.ToString();
        if (length != null && length > 0)
            str = str[..length.Value];
        
        if (withoutHyphens)
            str = str.Replace("-", "");
        
        return str;
    }
}
