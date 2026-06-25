namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI가 제공하는 서비스 타입을 나타냅니다.
/// </summary>
[Flags]
public enum OpenAIServiceType
{
    /// <summary>
    /// OpenAI의 모델 정보를 가져오는 서비스
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// LLM기반 채팅 서비스 (Messages API)
    /// </summary>
    Messages = 1 << 1,

    /// <summary>
    /// 임베딩 생성 서비스
    /// </summary>
    Embeddings = 1 << 2,

    /// <summary>
    /// 이미지 생성 서비스 (DALL-E)
    /// </summary>
    Images = 1 << 3,

    /// <summary>
    /// 오디오 처리 서비스 (TTS, Whisper)
    /// </summary>
    Audio = 1 << 4,

    /// <summary>
    /// 모든 OpenAI 서비스를 포함합니다.
    /// </summary>
    All = Models | Messages | Embeddings | Images | Audio,
}
