using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatRole
{
    User,
    Assistant
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public required ChatRole Role { get; set; }

    [JsonPropertyName("contents")]
    public required ICollection<ContentBlock> Contents { get; set; } = [];
}
