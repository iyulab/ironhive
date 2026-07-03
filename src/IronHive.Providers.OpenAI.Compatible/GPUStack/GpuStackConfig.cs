namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack service configuration. GPUStack serves OpenAI-compatible APIs at /v1-openai/.
/// </summary>
public class GpuStackConfig
{
    private const string DefaultBaseUrl = "http://localhost:8080";
    private const string ApiPath = "/v1-openai/";

    /// <summary>
    /// 인증용 API 키를 가져오거나 설정합니다.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// GPUStack server base URL (without path). Default: http://localhost:8080
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 매 요청 시 호출되어 BaseUrl을 동적으로 반환합니다.
    /// null이면 <see cref="BaseUrl"/>을 사용합니다.
    /// </summary>
    public Func<string>? BaseUrlResolver { get; set; }

    /// <summary>
    /// 매 요청 시 호출되어 API 키를 동적으로 반환합니다.
    /// null이면 정적 <see cref="ApiKey"/>를 사용합니다.
    /// 엔드포인트와 키는 보통 같은 설정 소스(DB/secret store)에서 함께 오므로,
    /// <see cref="BaseUrlResolver"/>와 대칭으로 동적 키 회전(rotation)을 지원합니다.
    /// </summary>
    public Func<string?>? ApiKeyResolver { get; set; }

    /// <summary>
    /// TCP 연결 타임아웃입니다. (Default: 2초)
    /// LAN 환경에서 정상 연결은 수십ms 이내이므로, unreachable 호스트에 대해
    /// OS 기본값(~21초) 대신 빠르게 실패하여 fallback 체인을 즉시 진행합니다.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// 현재 유효한 BaseUrl을 반환합니다.
    /// <see cref="BaseUrlResolver"/>가 설정된 경우 이를 우선 호출하며,
    /// 결과가 비어 있거나 null인 경우 <see cref="BaseUrl"/>로 fallback합니다.
    /// </summary>
    internal string ResolveBaseUrl()
    {
        if (BaseUrlResolver != null)
        {
            var resolved = BaseUrlResolver();
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;
        }
        return string.IsNullOrEmpty(BaseUrl) ? DefaultBaseUrl : BaseUrl;
    }

    /// <summary>
    /// 현재 유효한 API 키를 반환합니다.
    /// <see cref="ApiKeyResolver"/>가 설정된 경우 이를 우선 호출하며,
    /// 결과가 비어 있거나 null인 경우 정적 <see cref="ApiKey"/>로 fallback합니다.
    /// </summary>
    internal string ResolveApiKey()
    {
        if (ApiKeyResolver != null)
        {
            var resolved = ApiKeyResolver();
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;
        }
        return ApiKey ?? string.Empty;
    }

    /// <summary>
    /// Converts the current configuration to an equivalent <see cref="OpenAIConfig"/> instance.
    /// </summary>
    public OpenAIConfig ToOpenAI() => new()
    {
        BaseUrl = ResolveBaseUrl().TrimEnd('/') + ApiPath,
        ApiKey = ResolveApiKey(),
        HttpClient = new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = ConnectTimeout
        }),
    };
}
