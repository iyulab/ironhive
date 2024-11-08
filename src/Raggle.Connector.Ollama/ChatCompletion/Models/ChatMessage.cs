using System.Text.Json.Serialization;

namespace Raggle.Connector.Ollama.ChatCompletion.Models;

internal enum ChatRole
{
    System,
    User,
    Assistant,
    Tool
}

internal class ChatMessage
{
    [JsonPropertyName("role")]
    public required ChatRole Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("images")]
    public ICollection<string>? Images { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<ToolCall>? ToolCalls { get; set; }
}
