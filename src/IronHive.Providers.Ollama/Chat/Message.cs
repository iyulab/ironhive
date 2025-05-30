using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Chat;

internal enum MessageRole
{
    System,
    User,
    Assistant,
    Tool
}

internal class Message
{
    [JsonPropertyName("role")]
    public required MessageRole Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("images")]
    public ICollection<string>? Images { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<ToolCall>? ToolCalls { get; set; }
}
