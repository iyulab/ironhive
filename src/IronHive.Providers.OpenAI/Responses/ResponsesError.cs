using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

/// <summary>
/// 응답 중 에러가 있을 때 반환되는 구조
/// (OpenAI Error 객체 포맷)
/// </summary>
public class ResponsesError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
