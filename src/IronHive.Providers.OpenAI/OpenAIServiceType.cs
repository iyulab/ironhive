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
    /// LLM기반 채팅 서비스(Legacy)
    /// </summary>
    ChatCompletion = 1 << 1,

    /// <summary>
    /// LLM기반 채팅 서비스
    /// </summary>
    Responses = 1 << 2,

    /// <summary>
    /// 임베딩 생성 서비스
    /// </summary>
    Embeddings = 1 << 3,

    /// <summary>
    /// 이미지 생성 서비스 (DALL-E)
    /// </summary>
    Images = 1 << 4,

    /// <summary>
    /// 비디오 생성 서비스 (Sora)
    /// </summary>
    Videos = 1 << 5,

    /// <summary>
    /// 오디오 처리 서비스 (TTS, Whisper)
    /// </summary>
    Audio = 1 << 6,

    /// <summary>
    /// 모든 OpenAI 서비스를 포함합니다. 
    /// <para>
    /// 채팅용 LLM 서비스는 ChatCompletion이 아닌 Responses API로 설정됩니다.
    /// </para>
    /// </summary>
    All = Models | Responses | Embeddings | Images | Videos | Audio,
}