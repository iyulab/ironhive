using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class Message
{
    /// <summary>
    /// "user" or "assistant"
    /// </summary>
    [JsonPropertyName("role")]
    internal required string Role { get; set; }

    [JsonPropertyName("content")]
    internal required MessageContent[] Content { get; set; }
}
