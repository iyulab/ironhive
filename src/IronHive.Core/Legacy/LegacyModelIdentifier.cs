namespace IronHive.Core.Legacy;

/// <summary>
/// (Provider/Model) 문자열 파서 또는 변환기
/// </summary>
public class LegacyModelIdentifier
{
    private readonly char _separator;

    public LegacyModelIdentifier(char separator = '/')
    {
        _separator = separator;
    }

    public (string, string) Parse(string identifier)
    {
        var split = identifier.Split(_separator, 2, StringSplitOptions.TrimEntries);

        if (split.Length != 2)
            throw new ArgumentException($"Identifier must contain exactly one '{_separator}' character.");
        if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
            throw new ArgumentException("Identifier must not contain empty parts.");

        return (split[0], split[1]);
    }

    public string Stringify((string, string) model)
    {
        if (string.IsNullOrWhiteSpace(model.Item1) || string.IsNullOrWhiteSpace(model.Item2))
        {
            throw new ArgumentException("Model must not contain empty parts.");
        }
        return $"{model.Item1.Trim()}{_separator}{model.Item2.Trim()}";
    }
}
