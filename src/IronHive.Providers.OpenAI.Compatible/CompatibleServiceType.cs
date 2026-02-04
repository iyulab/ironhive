namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// OpenAI 호환 서비스 제공자가 지원하는 서비스 타입을 나타냅니다.
/// </summary>
[Flags]
public enum CompatibleServiceType
{
    /// <summary>
    /// LLM 기반 채팅 서비스 (Legacy Chat Completions API)
    /// </summary>
    ChatCompletion = 1 << 0,

    /// <summary>
    /// LLM 기반 채팅 서비스 (Responses API)
    /// xAI 등 Responses API를 지원하는 제공자용
    /// </summary>
    Responses = 1 << 1,

    /// <summary>
    /// 임베딩 생성 서비스
    /// </summary>
    Embeddings = 1 << 2,
}
