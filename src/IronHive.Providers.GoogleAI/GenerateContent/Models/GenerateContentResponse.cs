using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>
/// GenerateContent 응답 루트. 후보들, 프롬프트 피드백, 사용량 메타데이터 등을 포함합니다.
/// </summary>
internal sealed class GenerateContentResponse
{
    /// <summary>모델이 생성한 응답 후보 리스트.</summary>
    [JsonPropertyName("candidates")]
    public ICollection<ResponseCandidate>? Candidates { get; set; }

    /// <summary>프롬프트(입력) 필터링 결과(차단 사유/안전도 등).</summary>
    [JsonPropertyName("promptFeedback")]
    public PromptFeedback? PromptFeedback { get; set; }

    /// <summary>토큰 사용량 등의 메타데이터.</summary>
    [JsonPropertyName("usageMetadata")]
    public TokenUsages? UsageMetadata { get; set; }

    /// <summary>응답에 사용된 모델 버전 문자열.</summary>
    [JsonPropertyName("modelVersion")]
    public string? ModelVersion { get; set; }

    /// <summary>각 응답을 식별하는 ID.</summary>
    [JsonPropertyName("responseId")]
    public string? ResponseId { get; set; }
}