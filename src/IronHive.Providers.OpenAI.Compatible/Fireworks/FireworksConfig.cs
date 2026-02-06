namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스 설정입니다.
/// </summary>
public class FireworksConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.fireworks.ai/inference/v1";

    /// <summary>
    /// Top-K 샘플링
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 최소 확률 샘플링
    /// </summary>
    public float? MinP { get; set; }

    /// <summary>
    /// 반복 패널티
    /// </summary>
    public float? RepetitionPenalty { get; set; }

    /// <summary>
    /// 프롬프트 최대 길이 초과 시 잘라냄
    /// </summary>
    public int? PromptTruncateLen { get; set; }

    /// <summary>
    /// 컨텍스트 길이 초과 시 동작: "truncate" 또는 "error"
    /// </summary>
    public string? ContextLengthExceededBehavior { get; set; }

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
