namespace IronHive.Providers.Anthropic.Share;

/// <summary>
/// Anthropic이 제공하는 서비스들의 종류를 나타냅니다.
/// </summary>
[Flags]
public enum AnthropicServiceType
{
    /// <summary>
    /// Anthropic의 모델 정보를 제공하는 서비스입니다.
    /// </summary>
    Models = 1 << 0,

    /// <summary>
    /// Anthropic의 LLM기반 대화형 메시지 서비스를 제공합니다.
    /// </summary>
    Messages = 1 << 1,
}
