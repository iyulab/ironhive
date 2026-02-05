using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Groq;

/// <summary>
/// Groq 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class GroqMessageGenerator : CompatibleChatMessageGenerator
{
    public GroqMessageGenerator(GroqConfig config) : base(config)
    { }
}
