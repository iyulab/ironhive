using Google.Apis.Http;
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
    /// API 요청의 타임아웃 시간입니다.
    /// (Default: <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — 무제한)
    /// </summary>
    /// <remarks>
    /// 기본값(무제한)일 때는 요청 타임아웃을 두지 않습니다. 대신 <see cref="ConnectTimeout"/>이 TCP
    /// 연결 수립을 제한하므로, 응답이 없는 호스트에서 무한정 멈추지는 않습니다.
    /// <para>
    /// <see cref="HttpOptions"/>의 타임아웃과 함께 설정할 수 없습니다 — 둘 다 지정하면
    /// 어느 쪽이 이겼는지 알 수 없는 상태가 되므로 <see cref="InvalidOperationException"/>을 던집니다.
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
    /// Google AI API에 대한 HTTP 요청의 구성 옵션을 나타냅니다.
    /// BaseUrl과 같은 속성을 포함하여 API 엔드포인트를 사용자 정의할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 타임아웃만 조정하려면 <see cref="Timeout"/>을 쓰십시오 — 단위(밀리초)를 노출하지 않습니다.
    /// </remarks>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// Google API 클라이언트가 사용할 <see cref="HttpClient"/>를 생성하는 팩토리입니다.
    /// </summary>
    /// <remarks>
    /// 지정하지 않으면 어댑터가 <see cref="ConnectTimeout"/>을 적용하고
    /// <see cref="System.Net.Http.HttpClient.Timeout"/>을 무제한으로 설정한 기본 클라이언트를
    /// 생성합니다 — 벤더 SDK가 만드는 바닐라 <see cref="HttpClient"/>의 100초 기본값을 조용히
    /// 물려받지 않도록 하기 위함입니다.
    /// </remarks>
    public Func<HttpClient>? HttpClientFactory { get; set; }

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
