using Raggle.Abstractions.Json;
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
}
