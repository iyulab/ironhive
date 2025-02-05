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
}
