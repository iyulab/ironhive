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
    /// API 요청의 타임아웃 시간입니다. (Default: 10분)
    /// </summary>
    /// <remarks>
    /// 설정하지 않으면 <see cref="GoogleAIDefaults.Timeout"/>이 적용됩니다. 벤더 SDK는 타임아웃이
    /// 지정되지 않으면 <see cref="System.Net.Http.HttpClient"/>의 기본값 100초를 그대로 쓰는데,
    /// 그 값은 비스트리밍 호출에서는 응답 전체를, 스트리밍 호출에서는 첫 바이트까지의 시간을 제한한다.
    /// <para>
    /// <see cref="HttpOptions"/>의 타임아웃과 함께 설정할 수 없습니다 — 둘 다 지정하면
    /// 어느 쪽이 이겼는지 알 수 없는 상태가 되므로 <see cref="InvalidOperationException"/>을 던집니다.
    /// </para>
    /// </remarks>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Google AI API에 대한 HTTP 요청의 구성 옵션을 나타냅니다.
    /// BaseUrl과 같은 속성을 포함하여 API 엔드포인트를 사용자 정의할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// 타임아웃만 조정하려면 <see cref="Timeout"/>을 쓰십시오 — 단위(밀리초)를 노출하지 않습니다.
    /// </remarks>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// Google API 클라이언트의 동작을 구성하는 옵션을 나타냅니다
    /// </summary>
    public Func<HttpClient>? HttpClientFactory { get; set; }

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
