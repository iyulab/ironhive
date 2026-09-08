using Google.Apis.Auth.OAuth2;
using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google Vertex AI 서비스 연결을 위한 구성 클래스입니다.
/// </summary>
public class VertexAIConfig
{
    /// <summary>
    /// Google Cloud 인증 정보를 가져오거나 설정합니다.
    /// 이 속성은 필수이며 객체 초기화 시 반드시 제공되어야 합니다.
    /// </summary>
    public ICredential? Credential { get; set; }

    /// <summary>
    /// Google Cloud 프로젝트 ID를 가져오거나 설정합니다.
    /// Vertex AI 리소스가 속한 프로젝트를 식별하는 데 사용됩니다.
    /// </summary>
    public string? Project { get; set; }
    
    /// <summary>
    /// Vertex AI 서비스 리전(예: "us-central1", "asia-northeast3")을 가져오거나 설정합니다.
    /// 서비스가 실행될 지리적 위치를 지정합니다.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// API 요청의 타임아웃 시간입니다.
    /// (Default: <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — 무제한)
    /// </summary>
    /// <remarks>
    /// 기본값(무제한)일 때는 요청 타임아웃을 두지 않습니다. 대신 <see cref="ConnectTimeout"/>이 TCP
    /// 연결 수립을 제한하므로, 응답이 없는 호스트에서 무한정 멈추지는 않습니다.
    /// <para>
    /// <see cref="HttpOptions"/>의 타임아웃과 함께 설정할 수 없습니다 — 둘 다 지정하면
    /// <see cref="InvalidOperationException"/>을 던집니다.
    /// </para>
    /// </remarks>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// TCP 연결(connect) 타임아웃입니다. (Default: 5초)
    /// </summary>
    /// <remarks>
    /// <see cref="HttpClientFactory"/>를 직접 지정하면 이 값은 무시됩니다 — 연결 타임아웃은 그
    /// 팩토리가 생성하는 <see cref="HttpClient"/>의 책임이 됩니다.
    /// </remarks>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// HTTP 요청에 대한 추가 옵션을 가져오거나 설정합니다.
    /// BaseUrl 등 엔드포인트 관련 설정을 구성할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 타임아웃만 조정하려면 <see cref="Timeout"/>을 쓰십시오 — 단위(밀리초)를 노출하지 않습니다.
    /// </remarks>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// HTTP 클라이언트 팩토리를 가져오거나 설정합니다.
    /// 이 속성을 사용하면 사용자 정의 HTTP 클라이언트를 생성할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 지정하지 않으면 어댑터가 <see cref="ConnectTimeout"/>을 적용하고
    /// <see cref="System.Net.Http.HttpClient.Timeout"/>을 무제한으로 설정한 기본 클라이언트를
    /// 생성합니다.
    /// </remarks>
    public Func<HttpClient>? HttpClientFactory { get; set; }

    /// <summary>
    /// 구성이 유효한지 검증합니다.
    /// </summary>
    /// <returns>
    /// 모든 필수 속성(Credential, Project, Location)이 올바르게 설정되어 있으면 true,
    /// 그렇지 않으면 false를 반환합니다.
    /// </returns>
    public bool Validate()
    {
        return Credential != null 
            && !string.IsNullOrEmpty(Project) 
            && !string.IsNullOrEmpty(Location);
    }
}
