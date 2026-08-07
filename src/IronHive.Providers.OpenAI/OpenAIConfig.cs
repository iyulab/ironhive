namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI에 대한 설정 클래스입니다.
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// OpenAI API의 기본 URL을 가져오거나 설정합니다.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 계정의 조직 ID를 가져오거나 설정합니다.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI 프로젝트 ID를 가져오거나 설정합니다.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Http요청의 타임아웃을 가져오거나 설정합니다. (Default: 10분)
    /// </summary>
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// SDK가 사용할 HttpClient를 외부에서 주입합니다.
    /// <para>
    /// connect timeout, proxy, retry 등 HTTP 레벨 동작을 직접 제어할 때 사용합니다.
    /// <c>IHttpClientFactory</c>와 연동하여 DI 컨테이너에서 관리되는 HttpClient를 주입할 수도 있습니다.
    /// </para>
    /// <para>
    /// 주입하는 인스턴스의 <see cref="System.Net.Http.HttpClient.Timeout"/>은
    /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>으로 설정하십시오. 기본값 100초를 그대로 두면
    /// 그 값이 <see cref="TimeOut"/>보다 먼저 적용되어 첫 바이트 수신까지의 시간을 100초로 제한하며,
    /// <see cref="TimeOut"/> 설정은 무시된 것처럼 동작합니다. 주입하지 않으면 SDK 기본 전송 계층이
    /// 같은 이유로 이미 무제한을 사용하므로 이 문제가 없습니다.
    /// </para>
    /// </summary>
    public HttpClient? HttpClient { get; set; }

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
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
