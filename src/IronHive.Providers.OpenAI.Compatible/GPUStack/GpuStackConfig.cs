using IronHive.Abstractions.Http;

namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack service configuration. GPUStack serves its OpenAI-compatible APIs at the standard <c>/v1/</c>
/// (see <see cref="Path"/>).
/// </summary>
public class GpuStackConfig
{
    private const string DefaultBaseUrl = "http://localhost:8080";

    /// <summary>
    /// GPUStack serves its Jina-compatible rerank API (llama-box backend) at <c>/v1/rerank</c>,
    /// whatever <see cref="Path"/> says.
    /// </summary>
    private const string RerankApiPath = "/v1/";

    /// <summary>
    /// The path the OpenAI-compatible surface (chat completions, embeddings, models, images, audio) is served at,
    /// appended to the base URL. (Default: <c>/v1/</c>, the path GPUStack documents.) GPUStack also answers at its
    /// older <c>/v1-openai/</c>; set that here only for a GPUStack 0.x server that has nothing at <c>/v1/</c>. The
    /// standard path also lets the same configuration reach an OpenAI-compatible gateway put in front of GPUStack.
    /// Leading and trailing slashes are optional.
    /// </summary>
    public string Path { get; set; } = "/v1/";

    private string ApiPath
    {
        get
        {
            var trimmed = (Path ?? string.Empty).Trim().Trim('/');
            return trimmed.Length == 0 ? "/" : "/" + trimmed + "/";
        }
    }

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
    /// Where and how the API key is sent, passed through to the configurations this one converts to.
    /// (Default: <see cref="CredentialPlacement.Bearer"/>.) For a gateway in front of GPUStack that expects <c>Basic</c>,
    /// a bare token, or the key in its own header; <see cref="Headers"/> refuses <c>Authorization</c> and this
    /// placement's header.
    /// </summary>
    public CredentialPlacement ApiKeyPlacement { get; set; } = CredentialPlacement.Bearer;

    /// <summary>
    /// TCP 연결 타임아웃입니다. (Default: 3초)
    /// LAN 환경에서 정상 연결은 수십ms 이내이므로, unreachable 호스트에 대해
    /// OS 기본값(~21초) 대신 빠르게 실패하여 fallback 체인을 즉시 진행합니다.
    /// Windows 가 거부된 연결을 알리는 데 걸리는 ~2초보다 길게 두어, 꺼진 서버가 타임아웃이 아니라 거부로 보고되게 합니다.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(3);

    /// <inheritdoc cref="OpenAIConfig.Timeout"/>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <inheritdoc cref="OpenAIConfig.StreamIdleTimeout"/>
    public TimeSpan StreamIdleTimeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <inheritdoc cref="OpenAICompatibleConfig.TokenLimitParameter"/>
    public TokenLimitParameter TokenLimitParameter { get; set; } = TokenLimitParameter.MaxCompletionTokens;

    /// <inheritdoc cref="OpenAICompatibleConfig.CarryImageToolResultsAsUserMessage"/>
    public bool CarryImageToolResultsAsUserMessage { get; set; }

    /// <inheritdoc cref="OpenAICompatibleConfig.InstructionsAsSeparateSystemMessage"/>
    public bool InstructionsAsSeparateSystemMessage { get; set; }

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
    /// Converts the current configuration to an equivalent <see cref="OpenAIConfig"/> instance,
    /// targeting GPUStack's OpenAI-compatible surface (chat completions, embeddings, models) at
    /// <see cref="Path"/> (<c>/v1/</c> by default).
    /// </summary>
    public OpenAIConfig ToOpenAI() => ToOpenAIConfig(ApiPath);

    /// <summary>
    /// Extra request headers sent on every request — the uniform slot every IronHive provider config
    /// has, passed through to the configurations this one converts to. <c>Authorization</c> is refused
    /// and belongs to the API key.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// How many times a failed request is retried before it surfaces, passed through to the configurations this
    /// one converts to (<see cref="OpenAIConfig.MaxRetries"/>). Null keeps the SDK default (3); 0 sends one request.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Converts the current configuration to an equivalent <see cref="OpenAICompatibleConfig"/>, so
    /// GPUStack's Chat Completions surface can be served by the same
    /// <see cref="OpenAICompatibleMessageGenerator"/> that <see cref="OpenAICompatibleConfig"/> uses —
    /// GPUStack's dynamic base-URL/API-key resolution is otherwise identical, so a GPUStack-specific
    /// message generator would just duplicate it.
    /// </summary>
    /// <remarks>
    /// This is the public path for building a GPUStack chat generator outside the service registry:
    /// <c>new OpenAICompatibleMessageGenerator(config.ToOpenAICompatible())</c>. The registry's
    /// <c>AddGpuStackProviders</c> does exactly this; a consumer that wires generators directly
    /// (a gateway adapter, for example) needs the same conversion — <see cref="OpenAICompatibleConfig.Path"/> set to
    /// this <see cref="Path"/>, the resolvers, <see cref="ConnectTimeout"/> and
    /// <see cref="TokenLimitParameter"/> carried over — and should not have to re-derive it.
    /// </remarks>
    public OpenAICompatibleConfig ToOpenAICompatible() => new()
    {
        // Baked in as the static fallback; BaseUrlResolver/ApiKeyResolver below still take
        // precedence per request, so dynamic rotation is unaffected.
        BaseUrl = ResolveBaseUrl(),
        ApiKey = ResolveApiKey(),
        Path = ApiPath,
        BaseUrlResolver = BaseUrlResolver,
        ApiKeyResolver = ApiKeyResolver,
        ConnectTimeout = ConnectTimeout,
        Timeout = Timeout,
        StreamIdleTimeout = StreamIdleTimeout,
        TokenLimitParameter = TokenLimitParameter,
        CarryImageToolResultsAsUserMessage = CarryImageToolResultsAsUserMessage,
        InstructionsAsSeparateSystemMessage = InstructionsAsSeparateSystemMessage,
        Headers = Headers,
        ApiKeyPlacement = ApiKeyPlacement,
        MaxRetries = MaxRetries,
    };

    /// <summary>
    /// Converts the current configuration to an equivalent <see cref="OpenAIConfig"/> instance,
    /// targeting GPUStack's Jina-compatible rerank endpoint at <c>/v1/rerank</c> rather than
    /// <see cref="Path"/>.
    /// </summary>
    internal OpenAIConfig ToRerankConfig() => ToOpenAIConfig(RerankApiPath);

    private OpenAIConfig ToOpenAIConfig(string apiPath) => new()
    {
        BaseUrl = ResolveBaseUrl().TrimEnd('/') + apiPath,
        ApiKey = ResolveApiKey(),
        Headers = Headers,
        ApiKeyPlacement = ApiKeyPlacement,
        // No HttpClient here, as in OpenAICompatibleConfig.ToOpenAI: the receiving client creates and owns one.
        ConnectTimeout = ConnectTimeout,
        Timeout = Timeout,
        StreamIdleTimeout = StreamIdleTimeout,
        MaxRetries = MaxRetries,
    };
}
