using Raggle.Abstractions;

namespace Raggle.Core;

public class ModelIdentifierParser : ITextParser<(string, string)>
{
    private readonly char _separator;

    public ModelIdentifierParser(char separator = '/')
    {
        _separator = separator;
    }

    /// <summary>
    /// Parse model identifier to a service key and a model name.
    /// </summary>
    /// <returns>A tuple of (service key, model name)</returns>
    public (string, string) Parse(string identifier)
    {
        var split = identifier.Split(_separator, 2, StringSplitOptions.TrimEntries);

        if (split.Length != 2)
            throw new ArgumentException($"Identifier must contain exactly one '{_separator}' character.");
        if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
            throw new ArgumentException("Identifier must not contain empty parts.");
        
        return (split[0], split[1]);
    }

    /// <summary>
    /// Combine a service key and a model name to a model identifier.
    /// </summary>
    /// <param name="model">A tuple of (service key, model name)</param>
    public string Stringify((string, string) model)
    {
        if (string.IsNullOrWhiteSpace(model.Item1) || string.IsNullOrWhiteSpace(model.Item2))
        {
            throw new ArgumentException("Model must not contain empty parts.");
        }
        return $"{model.Item1.Trim()}{_separator}{model.Item2.Trim()}";
    }
}
