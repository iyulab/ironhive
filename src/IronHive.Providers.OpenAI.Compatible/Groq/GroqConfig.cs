namespace IronHive.Providers.OpenAI.Compatible.Groq;

/// <summary>
/// Groq 서비스 설정입니다.
/// </summary>
public class GroqConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.groq.com/openai/v1";

    /// <summary>
    /// 추론 모델(DeepSeek-R1, QwQ)에서 사용할 추론 형식입니다.
    /// "parsed": 구조화된 추론 결과, "raw": &lt;think&gt; 태그 인라인, "hidden": 추론은 수행하되 결과 미반환
    /// </summary>
    public string? ReasoningFormat { get; set; }

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
