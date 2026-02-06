namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스 설정입니다.
/// </summary>
public class TogetherAIConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.together.xyz/v1";

    /// <summary>
    /// Top-K 샘플링 (0-100)
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 반복 패널티 (0.0-2.0)
    /// </summary>
    public float? RepetitionPenalty { get; set; }

    /// <summary>
    /// 최소 확률 샘플링 (0.0-1.0)
    /// </summary>
    public float? MinP { get; set; }

    /// <summary>
    /// 콘텐츠 모더레이션 모델 (예: "Meta-Llama/LlamaGuard-2-8b")
    /// </summary>
    public string? SafetyModel { get; set; }

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
