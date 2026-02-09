namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek 서비스 설정입니다.
/// </summary>
public class DeepSeekConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.deepseek.com/v1";

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = this.ApiKey ?? string.Empty,
        };
    }
}
