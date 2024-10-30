using Raggle.Connector.OpenAI.Converters;
using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.Base;

/// <summary>
/// Represents an OpenAI model.
/// </summary>
internal class OpenAIModel
{
    /// <summary>
    /// The model ID. <br/>
    /// </summary>
    [JsonPropertyName("id")]
    internal string ID { get; set; } = string.Empty;

    /// <summary>
    /// The model owner.
    /// </summary>
    [JsonPropertyName("owned_by")]
    internal string OwnedBy { get; set; } = string.Empty;

    /// <summary>
    /// The creation date and time of the model.
    /// </summary>
    [JsonPropertyName("created")]
    [JsonConverter(typeof(UnixTimeJsonConverter))]
    internal DateTime Created { get; set; }

    internal static bool IsChatCompletionModel(OpenAIModel model)
    {
        return model.ID.Contains("gpt")
            || model.ID.Contains("babbage")
            || model.ID.Contains("davinci");
    }

    internal static bool IsEmbeddingModel(OpenAIModel model)
    {
        return model.ID.Contains("embedding");
    }

    internal static bool IsTextToImageModel(OpenAIModel model)
    {
        return model.ID.Contains("dall-e");
    }

    internal static bool IsTextToSpeechModel(OpenAIModel model)
    {
        return model.ID.Contains("tts");
    }

    internal static bool IsAudioToTextModel(OpenAIModel model)
    {
        return model.ID.Contains("whisper");
    }
}
