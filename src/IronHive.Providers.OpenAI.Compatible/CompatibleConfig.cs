namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// OpenAI 호환 서비스에 대한 기본 설정 클래스입니다.
/// 제공자별 특수 설정은 각 Provider Config 클래스를 사용하세요.
/// </summary>
public class CompatibleConfig
{
    /// <summary>
    /// 사용할 OpenAI 호환 서비스 제공자를 가져오거나 설정합니다.
    /// </summary>
    public CompatibleProvider Provider { get; set; } = CompatibleProvider.Custom;

    /// <summary>
    /// API 키를 가져오거나 설정합니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 사용자 정의 Base URL을 가져오거나 설정합니다.
    /// Provider가 Custom이거나 Self-hosted 서비스(vLLM, GPUStack)인 경우 필수입니다.
    /// 다른 Provider의 경우 설정하면 기본 URL을 덮어씁니다.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// API에 대한 요청에 사용할 기본 요청 헤더를 가져오거나 설정합니다.
    /// </summary>
    public IDictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
}
