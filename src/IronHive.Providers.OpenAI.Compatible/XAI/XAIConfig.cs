namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스 설정입니다.
/// </summary>
public class XAIConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.x.ai/v1";

    /// <summary>
    /// OpenAIConfig로 변환합니다.
    /// </summary>
    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = ApiKey ?? string.Empty,
        };
    }
}
