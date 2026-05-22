namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack service configuration. GPUStack serves OpenAI-compatible APIs at /v1-openai/.
/// </summary>
public class GpuStackConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "http://localhost:8080";
    private const string ApiPath = "/v1-openai/";

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

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI() => new()
    {
        BaseUrl = ResolveBaseUrl().TrimEnd('/') + ApiPath,
        ApiKey = ApiKey ?? string.Empty,
    };
}
