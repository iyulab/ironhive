namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스 설정입니다.
/// </summary>
public class FireworksConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.fireworks.ai/inference/v1";

    /// <summary>
    /// 서비스 계정 ID입니다.
    /// </summary>
    public string? AccountId { get; set; }

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
