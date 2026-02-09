namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek이 지원하는 서비스 유형입니다.
/// </summary>
[Flags]
public enum DeepSeekServiceType
{
    /// <summary>
    /// 모델 정보를 가져오는 서비스입니다.
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// LLM 응답 생성을 위한 서비스입니다.
    /// </summary>
    Language = 1 << 1,

    /// <summary>
    /// 모든 서비스를 포함합니다.
    /// </summary>
    All = Models | Language
}
