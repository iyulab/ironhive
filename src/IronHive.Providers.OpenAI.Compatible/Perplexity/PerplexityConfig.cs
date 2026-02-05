namespace IronHive.Providers.OpenAI.Compatible.Perplexity;

/// <summary>
/// Perplexity 서비스 설정입니다.
/// </summary>
public class PerplexityConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.perplexity.ai";

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
