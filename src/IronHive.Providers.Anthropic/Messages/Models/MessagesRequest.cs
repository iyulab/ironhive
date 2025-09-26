using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages.Models;

internal class MessagesRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required IEnumerable<Message> Messages { get; set; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    /// <summary>
    /// (Not Use) Container ID of the tool that is running in.
    /// </summary>
    [JsonPropertyName("container")]
    public string? Container { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("mcp_servers")]
    public IEnumerable<object>? McpServers { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    /// <summary>
    /// "auto", "standard_only"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("stop_sequences")]
    public IEnumerable<string>? StopSequences { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("thinking")]
    public ThinkingEffort? Thinking { get; set; }

    [JsonPropertyName("tool_choice")]
    public AnthropicToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<AnthropicTool>? Tools { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }
}
