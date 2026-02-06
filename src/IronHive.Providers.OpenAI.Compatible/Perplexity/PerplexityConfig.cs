namespace IronHive.Providers.OpenAI.Compatible.Perplexity;

/// <summary>
/// Perplexity 서비스 설정입니다.
/// </summary>
public class PerplexityConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.perplexity.ai";

    /// <summary>
    /// 검색 도메인 필터입니다 (예: ["wikipedia.org"], 제외: ["-reddit.com"]).
    /// </summary>
    public IList<string>? SearchDomainFilter { get; set; }

    /// <summary>
    /// 이미지 결과 반환 여부입니다 (기본: false).
    /// </summary>
    public bool? ReturnImages { get; set; }

    /// <summary>
    /// 관련 후속 질문 반환 여부입니다 (기본: false).
    /// </summary>
    public bool? ReturnRelatedQuestions { get; set; }

    /// <summary>
    /// 검색 시간 필터입니다: "month", "week", "day", "hour"
    /// </summary>
    public string? SearchRecencyFilter { get; set; }

    /// <summary>
    /// Top-K 샘플링 (0-2048)
    /// </summary>
    public int? TopK { get; set; }

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = ApiKey ?? string.Empty,
        };
    }
}
