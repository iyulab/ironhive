using Raggle.Abstractions.JsonConverters;
using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.Base;

/// <summary>
/// Represents an OpenAI model.
/// </summary>
internal class OpenAIModel
{
    /// <summary>
    /// The model ID. <br/>
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
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime Created { get; set; }

    internal static bool IsChatCompletionModel(OpenAIModel model)
    {
        return model.ID.Equals("o3-mini")
            //|| model.ID.Equals("o1")
            //|| model.ID.Equals("o1-mini")
            || model.ID.Equals("gpt-4o-mini")
            || model.ID.Equals("gpt-4o");
            //|| model.ID.Equals("gpt-4-turbo")
            //|| model.ID.Equals("gpt-4");
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
