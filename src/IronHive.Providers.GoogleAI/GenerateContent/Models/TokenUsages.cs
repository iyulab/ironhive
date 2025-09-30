using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>토큰 사용량 메타데이터.</summary>
internal sealed class TokenUsages
{
    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("promptTokenCount")]
    public int? PromptTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("cachedContentTokenCount")]
    public int? CachedContentTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("candidatesTokenCount")]
    public int? CandidatesTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("toolUsePromptTokenCount")]
    public int? ToolUsePromptTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("thoughtsTokenCount")]
    public int? ThoughtsTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("totalTokenCount")]
    public int? TotalTokenCount { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("promptTokensDetails")]
    public ICollection<ModalityTokenUsage>? PromptTokensDetails { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("cacheTokensDetails")]
    public ICollection<ModalityTokenUsage>? CacheTokensDetails { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("candidatesTokensDetails")]
    public ICollection<ModalityTokenUsage>? CandidatesTokensDetails { get; set; }

    /// <summary>총 토큰 수.</summary>
    [JsonPropertyName("toolUsePromptTokensDetails")]
    public ICollection<ModalityTokenUsage>? ToolUsePromptTokensDetails { get; set; }
}

internal class ModalityTokenUsage
{
    [JsonPropertyName("modality")]
    public Modality? Modality { get; set; }

    [JsonPropertyName("tokenCount")]
    public int? TokenCount { get; set; }
}