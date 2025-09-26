using System.Text.Json.Serialization;
using IronHive.Abstractions.Json;

namespace IronHive.Providers.Anthropic.Catalog.Models;

/// <summary>
/// Represents a Anthropic model.
/// </summary>
public class AnthropicModel
{
    /// <summary>
    /// Creation date and time of the model.
    /// </summary>
    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Display name of the model.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// the model ID.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// always "model"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
