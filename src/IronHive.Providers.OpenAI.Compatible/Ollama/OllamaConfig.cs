namespace IronHive.Providers.OpenAI.Compatible.Ollama;

/// <summary>
/// Ollama 서비스 설정입니다.
/// </summary>
public class OllamaConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "http://localhost:11434/v1";

    /// <summary>
    /// Ollama 서버의 기본 URL을 가져오거나 설정합니다.
    /// 기본값은 http://localhost:11434/v1 입니다.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <inheritdoc/>
    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = this.BaseUrl ?? DefaultBaseUrl,
            ApiKey = this.ApiKey ?? "ollama", // Ollama는 기본적으로 API 키가 불필요하지만, OpenAI 클라이언트 호환을 위해 더미 값 사용
        };
    }
}
