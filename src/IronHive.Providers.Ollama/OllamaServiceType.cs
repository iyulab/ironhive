namespace IronHive.Providers.Ollama;

/// <summary>
/// Ollama가 제공하는 서비스 종류입니다.
/// </summary>
[Flags]
public enum OllamaServiceType
{
    /// <summary>
    /// Ollama의 모델 정보를 제공하는 서비스입니다.
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// LLM기반 대화형 메시지 서비스를 제공합니다.
    /// </summary>
    Chat = 1 << 1,

    /// <summary>
    /// Embedding 서비스를 제공합니다.
    /// </summary>
    Embeddings = 1 << 2,

    /// <summary>
    /// 모든 Ollama 서비스 유형을 포함합니다.
    /// </summary>
    All = Models | Chat | Embeddings
}