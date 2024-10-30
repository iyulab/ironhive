using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class MessagesRequest
{
    [JsonPropertyName("model")]
    internal required string Model { get; set; }

    [JsonPropertyName("max_tokens")]
    internal required int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    internal required Message[] Messages { get; set; }

    [JsonPropertyName("system")]
    internal string? System { get; set; }

    [JsonPropertyName("tool_choice")]
    internal ToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    internal Tool[]? Tools { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    internal double? Temperature { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    [JsonPropertyName("top_k")]
    internal int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    internal double? TopP { get; set; }

    [JsonPropertyName("stop_sequences")]
    internal string[]? StopSequences { get; set; }

    [JsonPropertyName("metadata")]
    internal object? Metadata { get; set; }

    [JsonPropertyName("stream")]
    internal bool? Stream { get; set; }
}
