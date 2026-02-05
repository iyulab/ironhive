using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Perplexity;

/// <summary>
/// Perplexity 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class PerplexityMessageGenerator : CompatibleChatMessageGenerator
{
    public PerplexityMessageGenerator(PerplexityConfig config) : base(config)
    {
    }
}
