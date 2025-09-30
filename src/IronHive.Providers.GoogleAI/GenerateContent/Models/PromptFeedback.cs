using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

/// <summary>프롬프트(입력)에 대한 필터링 피드백.</summary>
internal sealed class PromptFeedback
{
    [JsonPropertyName("blockReason")]
    public BlockReason? BlockReason { get; set; }

    /// <summary>프롬프트 자체의 안전도 평가.</summary>
    [JsonPropertyName("safetyRatings")]
    public ICollection<SafetyRating>? SafetyRatings { get; set; }
}