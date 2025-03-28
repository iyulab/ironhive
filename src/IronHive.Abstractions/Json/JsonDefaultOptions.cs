using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Json;

/// <summary>
/// Global options for JsonSerializer.
/// </summary>
public static class JsonDefaultOptions
{
    public static JsonSerializerOptions Options { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,          // Avoids escaping slashes
        NumberHandling = JsonNumberHandling.AllowReadingFromString,     // Allows reading numbers from strings
        MaxDepth = 32,                                                  // Maximum depth of JSON to process
        WriteIndented = true,
    };

    public static JsonSerializerOptions CopyOptions => new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };
}
