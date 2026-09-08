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
    /// Anthropic API의 기본 URL입니다.
    /// </summary>
    public string? BaseUrl { get; set; }

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
    /// 추가적인 HTTP 요청 헤더를 설정할 수 있는 딕셔너리입니다.
    /// </summary>
    public IDictionary<string, string>? ExtraHeaders { get; set; }

    /// <summary>
    /// API 호출 실패 시 재시도할 최대 횟수입니다.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// API 요청의 타임아웃 시간입니다.
    /// (Default: <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — 무제한)
    /// </summary>
    /// <remarks>
    /// 기본값(무제한)일 때는 요청 타임아웃을 두지 않습니다. 대신 <see cref="ConnectTimeout"/>이 TCP
    /// 연결 수립을 제한하므로, 응답이 없는 호스트에서 무한정 멈추지는 않습니다.
    /// </remarks>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// TCP 연결(connect) 타임아웃입니다. (Default: 5초)
    /// </summary>
    /// <remarks>
    /// <see cref="HttpClient"/>를 직접 설정하면 이 값은 무시됩니다 — 연결 타임아웃은 그 클라이언트의
    /// 책임이 됩니다.
    /// </remarks>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 사용자 정의 <see cref="HttpClient"/>를 설정할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 지정하지 않으면 어댑터가 <see cref="ConnectTimeout"/>을 적용하고
    /// <see cref="System.Net.Http.HttpClient.Timeout"/>을 무제한으로 설정한 기본 클라이언트를
    /// 생성합니다 — 벤더 SDK가 만드는 바닐라 <see cref="HttpClient"/>의 100초 기본값을 조용히
    /// 물려받지 않도록 하기 위함입니다.
    /// </remarks>
    public HttpClient? HttpClient { get; set; }
    
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