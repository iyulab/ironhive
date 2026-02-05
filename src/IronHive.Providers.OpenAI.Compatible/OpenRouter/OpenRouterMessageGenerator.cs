using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.OpenRouter;

/// <summary>
/// OpenRouter 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class OpenRouterMessageGenerator : CompatibleChatMessageGenerator
{
    public OpenRouterMessageGenerator(OpenRouterConfig config) : base(config)
    { }
}
