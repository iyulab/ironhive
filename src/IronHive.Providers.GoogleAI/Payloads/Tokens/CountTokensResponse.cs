using IronHive.Providers.GoogleAI.Payloads.GenerateContent;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.Tokens;

internal class CountTokensResponse
{
    [JsonPropertyName("totalTokens")]
    public int? TotalTokens { get; set; }

    [JsonPropertyName("cachedContentTokenCount")]
    public int? CachedTokenCount { get; set; }

    [JsonPropertyName("promptTokensDetails")]
    public ICollection<ModalityTokenUsage>? PromptTokensDetails { get; set; }

    [JsonPropertyName("cacheTokensDetails")]
    public ICollection<ModalityTokenUsage>? CacheTokensDetails { get; set; }
}