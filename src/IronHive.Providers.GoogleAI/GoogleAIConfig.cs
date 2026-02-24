using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI 플랫폼 API에 연결하는 데 필요한 구성 설정을 나타냅니다.
/// </summary>
public class GoogleAIConfig
{
    /// <summary>
    /// Google AI API에 대한 요청을 인증하는 데 사용되는 API 키를 가져오거나 설정합니다.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Google AI API에 대한 HTTP 요청의 구성 옵션을 나타냅니다. 
    /// BaseUrl과 같은 속성을 포함하여 API 엔드포인트를 사용자 정의할 수 있습니다.
    /// </summary>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
