namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class XAIMessageGenerator : CompatibleResponseMessageGenerator
{
    public XAIMessageGenerator(XAIConfig config) : base(config)
    { }
}
