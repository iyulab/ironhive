using IronHive.Providers.GoogleAI.GenerateContent.Models;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Tokens.Models;

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