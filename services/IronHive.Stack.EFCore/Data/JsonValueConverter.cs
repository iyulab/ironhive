using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IronHive.Abstractions.Json;
using System.Text.Json;

namespace IronHive.Stack.WebApi.Data;

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

        return JsonSerializer.Serialize(value, JsonDefaultOptions.Options);
    }

    private static T? Deserialize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value, JsonDefaultOptions.Options);
        }
        catch
        {
            return default;
        }
    }
}
