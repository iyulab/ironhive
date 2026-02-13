namespace IronHive.Providers.Anthropic;

/// <summary>
/// Anthropic API에 대한 설정을 나타냅니다.
/// </summary>
public class AnthropicConfig
{
    /// <summary>
    /// Anthropic API의 기본 URL입니다.
    /// (Default: "https://api.anthropic.com")
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API 서비스의 인증 키입니다.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API key 유무를 검증합니다.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(ApiKey);
    }
}
