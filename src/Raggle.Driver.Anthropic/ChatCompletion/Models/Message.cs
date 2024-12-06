using System.Text.Json.Serialization;

namespace Raggle.Driver.Anthropic.ChatCompletion.Models;

internal class Message
{
    /// <summary>
    /// "user" or "assistant"
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public required ICollection<MessageContent> Content { get; set; }
}
