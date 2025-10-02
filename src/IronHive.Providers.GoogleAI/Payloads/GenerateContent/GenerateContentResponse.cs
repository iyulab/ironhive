using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class GenerateContentResponse
{
    /// <summary>
    /// 모델이 생성한 응답 후보 리스트.
    /// </summary>
    [JsonPropertyName("candidates")]
    public ICollection<ResponseCandidate>? Candidates { get; set; }

    /// <summary>
    /// 프롬프트(입력) 필터링 결과(차단 사유/안전도 등).
    /// </summary>
    [JsonPropertyName("promptFeedback")]
    public PromptFeedback? PromptFeedback { get; set; }

    /// <summary>
    /// 토큰 사용량 등의 메타데이터.
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public TokenUsages? UsageMetadata { get; set; }

    [JsonPropertyName("modelVersion")]
    public string? ModelVersion { get; set; }

    [JsonPropertyName("responseId")]
    public string? ResponseId { get; set; }
}