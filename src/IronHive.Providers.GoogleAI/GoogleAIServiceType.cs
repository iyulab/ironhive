namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI에서 제공하는 서비스 유형의 타입입니다.
/// </summary>
[Flags]
public enum GoogleAIServiceType
{
    /// <summary>
    /// 모델 정보를 가져오는 서비스 유형입니다.
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// LLM(대형 언어 모델) 관련 서비스 유형입니다.
    /// </summary>
    Messages = 1 << 1,

    /// <summary>
    /// 임베딩 생성 서비스 유형입니다.
    /// </summary>
    Embeddings = 1 << 2,

    /// <summary>
    /// 이미지 생성 서비스 유형입니다.
    /// </summary>
    Images = 1 << 3,

    /// <summary>
    /// 모든 Google AI 서비스 유형을 포함합니다.
    /// </summary>
    All = Models | Messages | Embeddings | Images
}