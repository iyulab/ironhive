using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Core.Utils;

public static class JsonDocumentSerializer
{
    private static readonly JsonSerializerOptions _defaultJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { 
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    public static string Serialize<T>(
        T value,
        JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(value, options ?? _defaultJsonOptions);
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Failed to serialize object");
        return json;
    }

    public static Stream SerializeToStream<T>(
        T value,
        JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(value, options ?? _defaultJsonOptions);
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Failed to serialize object");
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return stream;
    }

    public static T Deserialize<T>(
        string json,
        JsonSerializerOptions? options = null)
    {
        var value = JsonSerializer.Deserialize<T>(json, options ?? _defaultJsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize object");
        return value;
    }

    public static T Deserialize<T>(
        Stream stream,
        JsonSerializerOptions? options = null)
    {
        var value = JsonSerializer.Deserialize<T>(stream, options ?? _defaultJsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize object");
        return value;
    }

}
