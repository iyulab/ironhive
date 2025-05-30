using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages;

internal class CacheControl
{
    /// <summary>
    /// "ephemeral" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "ephemeral";
}
