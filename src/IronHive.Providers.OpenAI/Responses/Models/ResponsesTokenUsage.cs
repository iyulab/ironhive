using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses.Models;

internal class ResponsesTokenUsage
{
    [JsonPropertyName("input_tokens")]
    public required int InputTokens { get; set; }

    [JsonPropertyName("input_tokens_details")]
    public required ResponsesInputTokens InputTokensDetails { get; set; }

    [JsonPropertyName("output_tokens")]
    public required int OutputTokens { get; set; }

    [JsonPropertyName("output_tokens_details")]
    public required ResponsesOutputTokens OutputTokensDetails { get; set; }

    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; set; }
}

public class ResponsesInputTokens
{
    [JsonPropertyName("cached_tokens")]
    public required int CachedTokens { get; set; }
}

public class ResponsesOutputTokens
{
    [JsonPropertyName("reasoning_tokens")]
    public required int ReasoningTokens { get; set; }
}
