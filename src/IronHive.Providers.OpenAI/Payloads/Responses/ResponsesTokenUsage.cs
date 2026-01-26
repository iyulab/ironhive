using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

internal class ResponsesTokenUsage
{
    [JsonPropertyName("input_tokens")]
    public required int InputTokens { get; set; }

    [JsonPropertyName("input_tokens_details")]
    public required ResponsesInputTokensDetails InputTokensDetails { get; set; }

    [JsonPropertyName("output_tokens")]
    public required int OutputTokens { get; set; }

    [JsonPropertyName("output_tokens_details")]
    public required ResponsesOutputTokensDetails OutputTokensDetails { get; set; }

    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; set; }
}

public class ResponsesInputTokensDetails
{
    [JsonPropertyName("cached_tokens")]
    public required int CachedTokens { get; set; }
}

public class ResponsesOutputTokensDetails
{
    [JsonPropertyName("reasoning_tokens")]
    public required int ReasoningTokens { get; set; }
}
