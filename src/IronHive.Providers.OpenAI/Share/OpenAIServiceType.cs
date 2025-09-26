namespace IronHive.Providers.OpenAI.Share;

/// <summary>
/// OpenAI가 제공하는 서비스 타입을 나타내는 열거형입니다.
/// </summary>
[Flags]
public enum OpenAIServiceType
{
    /// <summary>
    /// OpenAI의 모델 정보를 가져오는 서비스
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// OpenAI의 LLM기반 채팅 서비스(Legacy)
    /// </summary>
    ChatCompletion = 1 << 1,

    /// <summary>
    /// OpenAI의 LLM기반 채팅 서비스
    /// </summary>
    Responses = 1 << 2,

    /// <summary>
    /// OpenAI의 임베딩 생성 서비스
    /// </summary>
    Embeddings = 1 << 3,
}
