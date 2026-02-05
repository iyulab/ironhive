namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스 설정입니다.
/// </summary>
public class FireworksConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.fireworks.ai/inference/v1";

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
