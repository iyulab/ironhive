using IronHive.Abstractions.Http;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI에 대한 설정 클래스입니다.
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// OpenAI API의 기본 URL을 가져오거나 설정합니다. 비워 두면 SDK 기본값(<c>https://api.openai.com/v1</c>)이 쓰입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// **버전 세그먼트를 포함한 완전한 엔드포인트**여야 합니다. 이 값은 벤더 SDK의 엔드포인트로 그대로
    /// 전달되며, 어댑터는 <c>/v1</c> 같은 경로를 붙이지 않습니다. 예를 들어
    /// <c>https://gateway.example.com</c> 을 설정하면 요청이 <c>/responses</c>·<c>/models</c> 로 나가
    /// 대상 서버에서 404가 되고, 올바른 값은 <c>https://gateway.example.com/v1</c> 입니다.
    /// </para>
    /// <para>
    /// ⚠️ 같은 이름의 <c>OpenAICompatibleConfig.BaseUrl</c>은 **반대 계약**입니다 — 그쪽은 «API 경로 없는
    /// 서버 주소»이고 <c>Path</c>(기본 <c>/v1</c>)를 어댑터가 덧붙입니다. 두 설정을 오가며 같은 값을
    /// 그대로 옮기면 한쪽에서 404가 됩니다. 경로를 자동으로 붙이지 않는 이유는 그 규칙이 호환 provider마다
    /// 다르기 때문입니다(예: GPUStack은 <c>/v1-openai</c>).
    /// </para>
    /// </remarks>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 모델(또는 모델 id 접두)별 능력 정책 덮어쓰기입니다. 내장 표(<see cref="OpenAIModelCapabilities.BuiltIn"/>)보다
    /// 우선하며, 내장 표에 없는 새 모델을 코드 수정 없이 선언할 때 씁니다.
    /// </summary>
    public IDictionary<string, OpenAIModelCapabilities>? ModelCapabilities { get; set; }

    /// <summary>
    /// OpenAI API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional resolver called on every request for the API key — a key kept in a secret store and rotated or
    /// revoked there takes effect on the next call, without rebuilding the provider. A null or blank answer falls
    /// back to <see cref="ApiKey"/>. Cannot be combined with a consumer-supplied <see cref="HttpClient"/>: the key is written by a
    /// handler in the HTTP client IronHive builds (construction throws <see cref="InvalidOperationException"/>).
    /// </summary>
    public Func<string?>? ApiKeyResolver { get; set; }

    /// <summary>
    /// Where and how the API key is sent. (Default: <see cref="CredentialPlacement.Bearer"/> —
    /// <c>Authorization: Bearer &lt;key&gt;</c>.)
    /// </summary>
    /// <remarks>
    /// For a gateway in front of an OpenAI-wire endpoint that expects another form:
    /// <c>CredentialPlacement.Authorization("Basic")</c>, <c>CredentialPlacement.Authorization(null)</c> for a bare token,
    /// or <c>CredentialPlacement.InHeader("api-key")</c>. The key stays in <see cref="ApiKey"/>/<see cref="ApiKeyResolver"/>;
    /// <see cref="Headers"/> refuses <c>Authorization</c> and this placement's header. Every request path reads it — the
    /// SDK path and this package's own HTTP clients (Chat Completions, rerank) — and a key sent elsewhere is not also
    /// sent as <c>Authorization: Bearer</c>. The key is sent as given: a <c>Basic</c> gateway takes the encoded
    /// <c>user:password</c>.
    /// </remarks>
    public CredentialPlacement ApiKeyPlacement { get; set; } = CredentialPlacement.Bearer;

    /// <summary>The key to construct the vendor client with: the resolver's answer, else <see cref="ApiKey"/>.</summary>
    internal string? ResolveApiKey()
    {
        var resolved = ApiKeyResolver?.Invoke();
        return string.IsNullOrWhiteSpace(resolved) ? ApiKey : resolved;
    }

    /// <summary>
    /// OpenAI 계정의 조직 ID를 가져오거나 설정합니다.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 프로젝트 ID를 가져오거나 설정합니다.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Http 요청의 타임아웃을 가져오거나 설정합니다.
    /// (Default: <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — 무제한)
    /// </summary>
    /// <remarks>
    /// <para>
    /// 기본값(무제한)일 때는 요청 타임아웃을 두지 않습니다. 대신 <see cref="ConnectTimeout"/>이 TCP
    /// 연결 수립을 제한하므로, 응답이 없는 호스트에서 무한정 멈추지는 않습니다.
    /// </para>
    /// <para>
    /// 스트리밍 요청에서는 <b>응답이 시작될 때까지</b>(응답 헤더)만 제한합니다 — 이미 흘러나오는 긴 답변을 자르지
    /// 않기 위해서입니다. 시작된 스트림이 멈춘 것을 잡으려면 <see cref="StreamIdleTimeout"/>을 씁니다.
    /// (Chat Completions 클라이언트 기준. Responses 클라이언트에서는 SDK의 <c>NetworkTimeout</c>으로 전달되어
    /// 네트워크 작업 하나 — 스트림이면 읽기 한 번 — 마다 적용됩니다.)
    /// </para>
    /// </remarks>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// 스트리밍 응답이 침묵할 수 있는 최대 시간 — 요청 후 첫 이벤트까지, 그리고 이벤트 사이.
    /// (Default: <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — 무제한)
    /// </summary>
    /// <remarks>
    /// 넘기면 스트림이 <see cref="TimeoutException"/>으로 끝납니다(메시지가 «stream idle timeout»을 말하므로 요청
    /// 타임아웃과 구별됩니다). 전체 요청 시한과 달리 느린 스트림(긴 프롬프트 평가 뒤 계속 흐르는 답변)과 죽은 스트림을
    /// 가릅니다 — 느린 하드웨어의 로컬 서버라면 첫 토큰 전 프롬프트 평가 시간보다 크게 잡습니다. 소비자가 이벤트를
    /// 처리하는 시간은 세지 않습니다. 버퍼링(비스트리밍) 요청에는 적용되지 않습니다.
    /// </remarks>
    public TimeSpan StreamIdleTimeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// TCP 연결(connect) 타임아웃입니다. (Default: 5초)
    /// </summary>
    /// <remarks>
    /// <see cref="HttpClient"/>를 직접 주입하면 이 값은 무시됩니다 — 연결 타임아웃은 주입한
    /// 클라이언트의 책임이 됩니다.
    /// </remarks>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// SDK가 사용할 HttpClient를 외부에서 주입합니다.
    /// <para>
    /// connect timeout, proxy, retry 등 HTTP 레벨 동작을 직접 제어할 때 사용합니다.
    /// <c>IHttpClientFactory</c>와 연동하여 DI 컨테이너에서 관리되는 HttpClient를 주입할 수도 있습니다.
    /// </para>
    /// <para>
    /// 주입하는 인스턴스의 <see cref="System.Net.Http.HttpClient.Timeout"/>은
    /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>으로 설정하십시오. 기본값 100초를 그대로 두면
    /// 그 값이 <see cref="Timeout"/>보다 먼저 적용되어 첫 바이트 수신까지의 시간을 100초로 제한하며,
    /// <see cref="Timeout"/> 설정은 무시된 것처럼 동작합니다. 주입하지 않으면 어댑터가
    /// <see cref="ConnectTimeout"/>을 적용하고 <see cref="System.Net.Http.HttpClient.Timeout"/>을
    /// 무제한으로 설정한 기본 클라이언트를 생성하므로 이 문제가 없습니다.
    /// </para>
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Extra request headers sent on every request this provider makes — a gateway's subscription key,
    /// a tenant or routing header. Applied at the transport, after the SDK has assembled the request,
    /// so a configured value wins over an SDK default of the same name.
    /// </summary>
    /// <remarks>
    /// The credential is not a header: <c>Authorization</c> is refused here and belongs to
    /// <see cref="ApiKey"/>. See <see cref="IronHive.Abstractions.Http.ProviderRequestHeaders"/> for the
    /// rules, which are the same for every provider.
    /// </remarks>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// How many times the SDK retries a failed request (a transport failure, 408, 429 or 5xx) before it surfaces.
    /// Null keeps the SDK default (3 retries, so up to 4 requests with backoff); 0 sends one request.
    /// </summary>
    /// <remarks>
    /// Same meaning as <c>AnthropicConfig.MaxRetries</c>. Set it to 0 where a caller retries itself or needs one
    /// prompt answer — a connection check, or an agent whose own retry layer would otherwise multiply the SDK's.
    /// </remarks>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// API key가 설정되어 있는지 확인합니다.
    /// </summary>
    /// <remarks>
    /// 이것은 "사용 가능한 설정인가"가 아니라 "자격증명이 있는가"에 대한 답입니다. 자격증명을 요구하지
    /// 않는 엔드포인트 — OpenAI 호환 로컬 서버, 또는 상류에서 자격증명을 주입하는 게이트웨이 — 는 키가
    /// 없어도 정상 동작하므로, 이 메서드가 <c>false</c>를 반환하는 것이 곧 오류를 뜻하지는 않습니다.
    /// 그래서 provider 등록 경로는 이것을 게이트로 쓰지 않습니다.
    /// </remarks>
    public bool Validate()
    {
        return ApiKeyResolver != null || !string.IsNullOrWhiteSpace(ApiKey);
    }
}
