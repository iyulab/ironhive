using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages.Models;

internal class CacheControl
{
    /// <summary>
    /// "ephemeral" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "ephemeral";

    /// <summary>
    /// "5m" or "1h" time to live, defaults to "5m".
    /// </summary>
    [JsonPropertyName("ttl")]
    public string TTL { get; set; } = "5m";
}
