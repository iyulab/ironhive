using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

internal class CacheControl
{
    /// <summary>
    /// "ephemeral" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "ephemeral";
}
