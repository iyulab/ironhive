using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal class TokenCountRequest
{
    [JsonPropertyName("messages")]
    public required ICollection<Message> Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("thinking")]
    public ThinkingConfig? Thinking { get; set; }

    [JsonPropertyName("tool_choice")]
    public AnthropicToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<AnthropicTool>? Tools { get; set; }
}
