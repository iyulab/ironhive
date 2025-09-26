﻿using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion.Models;

public class ChatCompletionRequest
{
    [JsonPropertyName("messages")]
    public required IEnumerable<ChatMessage> Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// (Not Use) when you use modalities: ["audio"]
    /// </summary>
    [JsonPropertyName("audio")]
    public ChatAudio? Audio { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// (Not Use) key is the token id, value is -100 to 100
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public IDictionary<int, int>? LogitBias { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("metadata")]
    public KeyValuePair<string, string>? Metadata { get; set; }

    /// <summary>
    /// (Not Use) output content type default is ["text"], audio model can ["text", "audio"]
    /// </summary>
    [JsonPropertyName("modalities")]
    public IEnumerable<string>? Modalities { get; set; }

    /// <summary>
    /// (Not Use) Generated Message Count of completions, default is 1
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("prediction")]
    public object? Prediction { get; set; }

    /// <summary>
    /// -2.0 to 2.0
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("prompt_cache_key")]
    public string? PromptCacheKey { get; set; }

    /// <summary>
    /// o-series model only
    /// </summary>
    [JsonPropertyName("reasoning_effort")]
    public ChatReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>
    /// (Not Use) text or json
    /// </summary>
    [JsonPropertyName("response_format")]
    public ChatResponseFormat? ResponseFormat { get; set; }

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("safety_identifier")]
    public string? SafetyIdentifier { get; set; }

    /// <summary>
    /// (Not Use) "auto", "default"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>
    /// Up to 4 sequences is available for stop generation
    /// </summary>
    [JsonPropertyName("stop")]
    public IEnumerable<string>? Stop { get; set; }

    /// <summary>
    /// store the output to OpenAI, default is false
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public ChatCompletionStreamOptions? StreamOptions { get; set; }

    /// <summary>
    /// 0.0 to 2.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// "none", "auto", ...
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<ChatTool>? Tools { get; set; }

    /// <summary>
    /// 0 to 20
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// Constrains the verbosity of the model's response
    /// </summary>
    [JsonPropertyName("verbosity")]
    public ChatVerbosity? Verbosity { get; set; }

    /// <summary>
    /// (Not Use) web search tool
    /// </summary>
    [JsonPropertyName("web_search_options")]
    public object? WebSearchOptions { get; set; }
}

public class ChatCompletionStreamOptions
{
    [JsonPropertyName("include_obfuscation")]
    public bool? IncludeObfuscation { get; set; }

    [JsonPropertyName("include_usage")]
    public bool? IncludeUsage { get; set; }
}