namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스 설정입니다.
/// </summary>
public class TogetherAIConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.together.xyz/v1";

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
