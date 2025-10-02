using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class ThinkingConfig
{
    /// <summary>
    /// 응답에 사고 내용을 포함할지 여부(지원 모델 한정).
    /// </summary>
    [JsonPropertyName("includeThoughts")]
    public required bool IncludeThoughts { get; set; }

    /// <summary>사고 토큰 예산.</summary>
    [JsonPropertyName("thinkingBudget")]
    public int? ThinkingBudget { get; set; }
}