using IronHive.Abstractions.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Catalog;

/// <summary>
/// Represents an OpenAI model.
/// </summary>
public class OpenAIModel
{
    /// <summary>
    /// always "model"
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "model";

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
    public DateTime? Created { get; set; }
}
