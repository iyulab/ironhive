namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek 서비스 설정입니다.
/// </summary>
public class DeepSeekConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.deepseek.com/v1";

    /// <summary>
    /// Thinking 모드를 활성화합니다 (deepseek-reasoner 모델 전용).
    /// </summary>
    public bool EnableThinking { get; set; }

    /// <summary>
    /// Thinking 모드에서 추론에 사용할 최대 토큰 수입니다 (최소 1024, 최대 65536).
    /// </summary>
    public int? ThinkingBudgetTokens { get; set; }

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
