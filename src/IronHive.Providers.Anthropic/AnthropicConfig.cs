namespace IronHive.Providers.Anthropic;

/// <summary>
/// Anthropic API 호출에 필요한 설정 정보를 나타냅니다.
/// </summary>
/// <remarks>
/// 인증 방식은 다음 두 가지 중 하나를 사용할 수 있습니다.
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="ApiKey"/>: 기본 인증 방식. HTTP 헤더 <c>x-api-key</c> 로 전송됩니다.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="AuthToken"/>: Bearer 토큰 방식. HTTP 헤더 
/// <c>Authorization: Bearer &lt;token&gt;</c> 로 전송됩니다.
/// </description>
/// </item>
/// </list>
/// 두 값 중 하나는 반드시 설정되어야 합니다.
/// </remarks>
public class AnthropicConfig
{
    /// <summary>
    /// Anthropic API 인증에 사용되는 API Key입니다.
    /// </summary>
    /// <remarks>
    /// HTTP 요청 시 <c>x-api-key</c> 헤더에 포함되어 전송됩니다.
    /// <see cref="AuthToken"/>이 설정되지 않은 경우 필수입니다.
    /// </remarks>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Anthropic API 인증에 사용되는 Bearer 토큰입니다.
    /// </summary>
    /// <remarks>
    /// HTTP 요청 시 <c>Authorization: Bearer &lt;token&gt;</c> 헤더에 포함되어 전송됩니다.
    /// <see cref="ApiKey"/>가 설정되지 않은 경우 필수입니다.
    /// </remarks>
    public string? AuthToken { get; set; }

    /// <summary>
    /// Anthropic API의 기본 URL입니다.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// API 호출 실패 시 재시도할 최대 횟수입니다.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// API 요청의 타임아웃 시간입니다.
    /// </summary>d
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// 설정 값의 유효성을 검증합니다.
    /// </summary>
    /// <returns>
    /// <see cref="ApiKey"/> 또는 <see cref="AuthToken"/> 중 
    /// 하나 이상이 유효한 값이면 <c>true</c>, 그렇지 않으면 <c>false</c>.
    /// </returns>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey)
            || !string.IsNullOrWhiteSpace(AuthToken);
    }
}