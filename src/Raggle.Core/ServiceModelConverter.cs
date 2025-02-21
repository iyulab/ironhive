using Raggle.Abstractions;

namespace Raggle.Core;

public class ServiceModelConverter : IServiceModelConverter
{
    public string Format(string service, string model)
    {
        if (string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Service and model must not be empty.");
        }
        return $"{service}/{model}";
    }

    public (string, string) Parse(string identifier)
    {
        var split = identifier.Split('/', 2, StringSplitOptions.TrimEntries);
        if (split.Length != 2)
        {
            throw new ArgumentException("Identifier must contain exactly one '/' character.");
        }
        if (string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
        {
            throw new ArgumentException("Identifier must not contain empty parts.");
        }
        return (split[0], split[1]);
    }
}
