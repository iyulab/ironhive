namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스 설정입니다.
/// </summary>
public class TogetherAIConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.together.xyz/v1";

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
