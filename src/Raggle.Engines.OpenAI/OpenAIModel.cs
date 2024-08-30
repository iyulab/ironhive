using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Raggle.Engines.OpenAI;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OpenAIModelType
{
    /// <summary>
    /// Represents the GPT Chat model.
    /// </summary>
    GPT,

    /// <summary>
    /// Represents the Embeddings model.
    /// </summary>
    Embeddings,

    /// <summary>
    /// Represents the Dalle (Text-to-Image) model.
    /// </summary>
    Dall_e,

    /// <summary>
    /// Represents the TTS (Text-to-Speech) model.
    /// </summary>
    TTS,

    /// <summary>
    /// Represents the Whisper (Audio-to-Text) model.
    /// </summary>
    Whisper,

    /// <summary>
    /// Represents the GPT Base (legacy text-generation) model.
    /// </summary>
    GPT_Base,

    /// <summary>
    /// Represents an unknown (not specified this library) model type.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents an OpenAI model.
/// </summary>
public class OpenAIModel
{
    /// <summary>
    /// The model type.
    /// </summary>
    public OpenAIModelType Type => GetModelType(ID);

    /// <summary>
    /// The model ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// The model owner.
    /// </summary>
    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;

    /// <summary>
    /// The creation date and time of the model.
    /// </summary>
    [JsonPropertyName("created")]
    [JsonConverter(typeof(UnixTimeToDateTimeConverter))]
    public DateTime Created { get; set; }

    private static readonly Dictionary<string, OpenAIModelType> ModelTypeMappings = new()
    {
        { "gpt", OpenAIModelType.GPT },
        { "embedding", OpenAIModelType.Embeddings },
        { "dall-e", OpenAIModelType.Dall_e },
        { "whisper", OpenAIModelType.Whisper },
        { "tts", OpenAIModelType.TTS },
        { "babbage", OpenAIModelType.GPT_Base },
        { "davinci", OpenAIModelType.GPT_Base }
    };

    /// <summary>
    /// Maps the model ID to a specific model type.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <returns>The corresponding OpenAIModelType.</returns>
    private static OpenAIModelType GetModelType(string modelId)
    {
        foreach (var mapping in ModelTypeMappings)
        {
            if (Regex.IsMatch(modelId, mapping.Key, RegexOptions.IgnoreCase))
            {
                return mapping.Value;
            }
        }
        return OpenAIModelType.Unknown;
    }
}

/// <summary>
/// Converts Unix time (seconds since epoch) to DateTime.
/// </summary>
public class UnixTimeToDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var unixTime = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}
