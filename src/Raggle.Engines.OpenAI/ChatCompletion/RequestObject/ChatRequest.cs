using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

public class ChatRequest
{
    [JsonPropertyName("messages")]
    public required ICollection<Message> Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    // -2.0 to 2.0
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    // value -100 to 100
    [JsonPropertyName("logit_bias")]
    public IDictionary<int, int>? LogitBias { get; set; }

    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    // 0 to 20
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("n")]
    public int? N { get; set; }

    // -2.0 to 2.0
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("stop")]
    public ICollection<string>? Stop { get; set; }

    // 0 to 2
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("tools")]
    public ICollection<Tool>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public object? StreamOptions { get; set; }
}
