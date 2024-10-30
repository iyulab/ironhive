using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class CacheControl
{
    /// <summary>
    /// "ephemeral" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "ephemeral";
}
