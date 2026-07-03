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
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
