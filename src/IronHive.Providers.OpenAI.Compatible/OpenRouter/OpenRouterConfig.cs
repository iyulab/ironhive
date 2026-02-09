namespace IronHive.Providers.OpenAI.Compatible.OpenRouter;

/// <summary>
/// OpenRouter 서비스 설정입니다.
/// </summary>
public class OpenRouterConfig : CompatibleConfig
{
    private const string DefaultBaseUrlValue = "https://openrouter.ai/api/v1";

    /// <summary>
    /// 리더보드 및 분석에서 앱을 식별하는 데 사용됩니다.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// 리더보드에 표시되는 앱 이름입니다.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Provider 선호도 설정입니다.
    /// </summary>
    public OpenRouterProviderPreferences? ProviderPreferences { get; set; }

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI()
    {
        var headers = new Dictionary<string, string>();

        // HTTP-Referer: 앱 URL (리더보드 및 분석용)
        if (!string.IsNullOrEmpty(SiteUrl))
            headers["HTTP-Referer"] = SiteUrl;

        // X-Title: 앱 이름 (리더보드 표시용)
        if (!string.IsNullOrEmpty(AppName))
            headers["X-Title"] = AppName;

        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrlValue,
            ApiKey = ApiKey ?? string.Empty,
            DefaultHeaders = headers
        };
    }
}

/// <summary>
/// OpenRouter Provider 선호도 설정입니다.
/// </summary>
public class OpenRouterProviderPreferences
{
    /// <summary>
    /// 실패 시 다른 Provider로 폴백을 허용할지 여부입니다.
    /// </summary>
    public bool? AllowFallbacks { get; set; }

    /// <summary>
    /// 요청된 파라미터를 지원하는 Provider만 사용할지 여부입니다.
    /// </summary>
    public bool? RequireParameters { get; set; }

    /// <summary>
    /// Provider 우선순위 목록입니다.
    /// </summary>
    public IList<string>? Order { get; set; }

    /// <summary>
    /// 사용하지 않을 Provider 목록입니다.
    /// </summary>
    public IList<string>? Ignore { get; set; }

    /// <summary>
    /// 선호 양자화 레벨입니다 ("fp32", "fp16", "bf16", "int8", "int4").
    /// </summary>
    public IList<string>? Quantizations { get; set; }

    /// <summary>
    /// 데이터 수집 제어입니다: "allow" 또는 "deny"
    /// </summary>
    public string? DataCollection { get; set; }

    /// <summary>
    /// 정렬 기준입니다: "price", "throughput", "latency"
    /// </summary>
    public string? Sort { get; set; }
}
