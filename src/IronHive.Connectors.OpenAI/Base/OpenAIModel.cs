using IronHive.Abstractions.Json;
using System.Text.Json.Serialization;

namespace IronHive.Connectors.OpenAI.Base;

/// <summary>
/// Represents an OpenAI model.
/// </summary>
internal class OpenAIModel
{
    /// <summary>
    /// The model ID. <br/>
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The model owner.
    /// </summary>
    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;

    /// <summary>
    /// The creation date and time of the model.
    /// </summary>
    [JsonPropertyName("created")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime Created { get; set; }

    public bool IsChatCompletion()
    {
        // 최신 모델 및 Alias 모델만 추가
        return Id.Equals("o3-mini")
            || Id.Equals("o1")
            || Id.Equals("o1-mini")
            || Id.Equals("gpt-4o-mini")
            || Id.Equals("gpt-4o");
    }

    public bool IsEmbedding()
    {
        return Id.Contains("embedding");
    }

    public bool IsTextToImage()
    {
        return Id.Contains("dall-e");
    }

    public bool IsTextToSpeech()
    {
        return Id.Contains("tts");
    }

    public bool IsAudioToText()
    {
        return Id.Contains("whisper");
    }
}
