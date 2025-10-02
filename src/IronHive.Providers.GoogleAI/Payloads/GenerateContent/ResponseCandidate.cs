using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class ResponseCandidate
{
    /// <summary>생성된 콘텐츠(파트 모음).</summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public FinishReason? FinishReason { get; set; }

    /// <summary>후보의 안전도 평가(카테고리별 0~1개).</summary>
    [JsonPropertyName("safetyRatings")]
    public ICollection<SafetyRating>? SafetyRatings { get; set; }

    /// <summary>인용/출처 메타데이터(코드 라이선스 등 포함 가능).</summary>
    [JsonPropertyName("citationMetadata")]
    public CitationMetadata? CitationMetadata { get; set; }

    /// <summary> 후보의 토큰 수.</summary>
    [JsonPropertyName("tokenCount")]
    public int? TokenCount { get; set; }

    /// <summary>그라운딩(검색 등) 데이터.</summary>
    [JsonPropertyName("groundingAttributions")]
    public ICollection<GroundingData>? GroundingAttributions { get; set; }

    /// <summary>그라운딩(검색 등) 메타데이터.</summary>
    [JsonPropertyName("groundingMetadata")]
    public GroundingMetadata? GroundingMetadata { get; set; }

    /// <summary>평균 토큰 로그확률.</summary>
    [JsonPropertyName("avgLogprobs")]
    public float? AvgLogprobs { get; set; }

    /// <summary>토큰별 로그확률 상위 후보 등.</summary>
    [JsonPropertyName("logprobsResult")]
    public LogprobsResult? LogprobsResult { get; set; }

    /// <summary>URL 컨텍스트 도구 관련 메타데이터.</summary>
    [JsonPropertyName("urlContextMetadata")]
    public UrlContextMetadata? UrlContextMetadata { get; set; }

    /// <summary>이 후보의 인덱스.</summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}