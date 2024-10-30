using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class ChatCompletionRequest
{
    [JsonPropertyName("messages")]
    internal required Message[] Messages { get; set; }

    [JsonPropertyName("model")]
    internal required string Model { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    internal double? FrequencyPenalty { get; set; }

    /// <summary>
    /// key is the token id, value is -100 to 100
    /// </summary>
    [JsonPropertyName("logit_bias")]
    internal IDictionary<int, int>? LogitBias { get; set; }

    [JsonPropertyName("logprobs")]
    internal bool? LogProbs { get; set; }

    /// <summary>
    /// 0 to 20
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    internal int? TopLogProbs { get; set; }

    [JsonPropertyName("max_tokens")]
    internal int? MaxTokens { get; set; }

    /// <summary>
    /// Generated Message Count of completions
    /// </summary>
    [JsonPropertyName("n")]
    internal int? N { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    internal double? PresencePenalty { get; set; }

    [JsonPropertyName("response_format")]
    internal ResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("seed")]
    internal int? Seed { get; set; }

    /// <summary>
    /// "auto", "default"
    /// </summary>
    [JsonPropertyName("service_tier")]
    internal string? ServiceTier { get; set; }

    /// <summary>
    /// Up to 4 sequences is available
    /// </summary>
    [JsonPropertyName("stop")]
    internal string[]? Stop { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    internal double? Temperature { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    internal double? TopP { get; set; }

    [JsonPropertyName("tools")]
    internal Tool[]? Tools { get; set; }

    /// <summary>
    /// "none", "auto", <see cref="Tool"/>
    /// </summary>
    [JsonPropertyName("tool_choice")]
    internal object? ToolChoice { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    internal bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("user")]
    internal string? User { get; set; }

    [JsonPropertyName("stream")]
    internal bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    internal StreamOptions? StreamOptions { get; set; }
}

internal class StreamOptions
{
    [JsonPropertyName("include_usage")]
    internal bool? InCludeUsage { get; set; }
}
