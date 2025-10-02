using System.Text.Json.Serialization;
using IronHive.Abstractions.Json;

namespace IronHive.Providers.Anthropic.Payloads.Models;

public class AnthropicModel
{
    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// always "model"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "model";
}