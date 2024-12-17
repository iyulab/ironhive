using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions;

/// <summary>
/// Global options for Raggle.
/// </summary>
public static class RaggleOptions
{
    public static JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
        WriteIndented = true,
    };
}
