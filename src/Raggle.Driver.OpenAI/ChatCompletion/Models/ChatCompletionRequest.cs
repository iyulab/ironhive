using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion.Models;

internal class ChatCompletionRequest
{
    [JsonPropertyName("messages")]
    public required Message[] Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// key is the token id, value is -100 to 100
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public IDictionary<int, int>? LogitBias { get; set; }

    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    /// <summary>
    /// 0 to 20
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Generated Message Count of completions
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// "auto", "default"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>
    /// Up to 4 sequences is available
    /// </summary>
    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }

    /// <summary>
    /// "none", "auto", <see cref="Tool"/>
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public StreamOptions? StreamOptions { get; set; }
}

internal class StreamOptions
{
    [JsonPropertyName("include_usage")]
    public bool? InCludeUsage { get; set; }
}
