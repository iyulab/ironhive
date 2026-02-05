namespace IronHive.Providers.OpenAI.Compatible.Fireworks;

/// <summary>
/// Fireworks AI 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class FireworksMessageGenerator : CompatibleChatMessageGenerator
{
    public FireworksMessageGenerator(FireworksConfig config) : base(config)
    { }
}
