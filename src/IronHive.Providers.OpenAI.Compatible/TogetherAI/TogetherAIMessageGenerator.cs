namespace IronHive.Providers.OpenAI.Compatible.TogetherAI;

/// <summary>
/// Together AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class TogetherAIMessageGenerator : CompatibleChatMessageGenerator
{
    public TogetherAIMessageGenerator(TogetherAIConfig config) : base(config)
    { }
}
