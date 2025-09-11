namespace IronHive.Providers.Anthropic;

/// <summary>
/// Anthropic이 제공하는 서비스들의 종류를 나타냅니다.
/// </summary>
public enum AnthropicServiceType
{
    None = 0,

    Messages = 1 << 0,
    Models = 1 << 1,
    
    All = Messages | Models
}
