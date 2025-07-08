using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Embedding;

public class OpenAIEmbeddingRequest
{
    [JsonPropertyName("input")]
    [JsonConverter(typeof(TextInputJsonConverter))]
    public required IEnumerable<string> Input { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// support "base64", "float"
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; } = "float";

    /// <summary>
    /// Only 'text-embedding-3' later model works
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

/// <summary>
/// Json converter for OpenAIEmbeddingRequest's input property.
/// </summary>
public class TextInputJsonConverter : JsonConverter<IEnumerable<string>>
{
    public override IEnumerable<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new[] { reader.GetString()! };
        }
        else
        {
            return JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<string> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}