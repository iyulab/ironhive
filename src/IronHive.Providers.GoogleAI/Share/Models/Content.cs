using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Share.Models;

/// <summary>
/// Google AI의 메시지 콘텐츠 모델.
/// </summary>
internal sealed class Content
{
    /// <summary>
    /// "user" 또는 "model" 역할.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// 이 턴을 구성하는 파트(텍스트/미디어/함수콜 등) 목록.
    /// </summary>
    [JsonPropertyName("parts")]
    public ICollection<ContentPart> Parts { get; set; } = [];
}