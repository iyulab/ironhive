namespace IronHive.Providers.OpenAI.Compatible.Groq;

/// <summary>
/// Groq 서비스 설정입니다.
/// </summary>
public class GroqConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.groq.com/openai/v1";

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
