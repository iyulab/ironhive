using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI 플랫폼 API에 연결하는 데 필요한 구성 설정을 나타냅니다.
/// </summary>
public class GoogleAIConfig
{
    /// <summary>
    /// Google AI API의 기본 URL을 가져오거나 설정합니다.
    /// 기본값은 "https://generativelanguage.googleapis.com/v1beta/".
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Google AI API에 대한 요청을 인증하는 데 사용되는 API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Google API 요청, 응답의 Json 직렬화 옵션을 가져오거나 설정합니다.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        WriteIndented = true // 디버깅을 위해 들여쓰기 설정
    };

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}