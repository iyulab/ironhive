using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Raggle.Abstractions;
using System.Text.Json;

namespace Raggle.Server.Data;

public class JsonValueConverter<T> : ValueConverter<T?, string?>
{
    public JsonValueConverter()
        : base(
            v => Serialize(v),
            v => Deserialize(v))
    {
    }

    private static string? Serialize(T? value)
    {
        if (value == null)
            return null;

        return JsonSerializer.Serialize(value, RaggleOptions.JsonOptions);
    }

    private static T? Deserialize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value, RaggleOptions.JsonOptions);
        }
        catch
        {
            return default;
        }
    }
}