namespace System;

public static class StringExtensions
{
    /// <summary>
    /// 문자열의 접미사를 지정한 값으로 만듭니다.
    /// </summary>
    public static string EnsureSuffix(this string str, string value)
        => str.EndsWith(value) ? str : str + value;

    /// <summary>
    /// 문자열의 접미사를 지정한 값으로 만듭니다.
    /// </summary>
    public static string EnsureSuffix(this string str, char value)
        => str.EndsWith(value) ? str : str + value;

    /// <summary>
    /// 문자열이 null 또는 공백인 경우 지정한 값으로 대체합니다.
    /// </summary>
    public static string OrDefault(this string? str, string value)
        => string.IsNullOrWhiteSpace(str) ? value : str;
}
