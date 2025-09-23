using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

internal class ResponsesTokenUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("input_tokens_details")]
    public int InputTokensDetails { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonPropertyName("output_tokens_details")]
    public ResponsesOutputTokens? OutputTokensDetails { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class ResponsesInputTokens
{
    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}

public class ResponsesOutputTokens
{
    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }
}
