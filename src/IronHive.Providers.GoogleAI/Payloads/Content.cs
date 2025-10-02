using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads;



/// <summary>
/// Google AI의 메시지 콘텐츠 모델.
/// </summary>
internal sealed class Content
{
    [JsonPropertyName("role")]
    public ContentRole? Role { get; set; }

    /// <summary>
    /// 이 턴을 구성하는 파트(텍스트/미디어/함수콜 등) 목록.
    /// </summary>
    [JsonPropertyName("parts")]
    public ICollection<ContentPart> Parts { get; set; } = [];

    public Content(ContentRole? role = null)
    {
        Role = role;
    }
}

/// <summary>
/// Google AI의 메시지 역할. CamelCase
/// </summary>
internal enum ContentRole
{
    user,
    model
}