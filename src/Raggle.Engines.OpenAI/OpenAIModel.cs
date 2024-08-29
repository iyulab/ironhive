using System.Text.Json.Serialization;

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
    Dalle,

    /// <summary>
    /// Represents the TTS (Text-to-Speech) model.
    /// </summary>
    TTS,

    /// <summary>
    /// Represents the Whisper model.
    /// </summary>
    Whisper,

    /// <summary>
    /// Represents the GPTBase model.
    /// </summary>
    GPTBase,

    /// <summary>
    /// Represents an unknown model type.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents an OpenAI model.
/// </summary>
public class OpenAIModel
{
    /// <summary>
    /// the model type.
    /// </summary>
    public OpenAIModelType Type { get; set; }

    /// <summary>
    /// the model ID.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// the creation date and time of the model.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
