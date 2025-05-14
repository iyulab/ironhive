namespace System;

public static class GuidExtensions
{
    public static string ToShort(this Guid guid, int? length = null, bool withoutHyphens = true)
    {
        var str = guid.ToString();
        if (length != null && length > 0)
        {
            str = str.Substring(0, length.Value);
        }
        if (withoutHyphens)
        {
            str = str.Replace("-", "");
        }
        return str;
    }
}
