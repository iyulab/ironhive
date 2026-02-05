using System.Text.Json.Nodes;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.DeepSeek;

/// <summary>
/// DeepSeek 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class DeepSeekMessageGenerator : CompatibleChatMessageGenerator
{
    public DeepSeekMessageGenerator(DeepSeekConfig config): base(config)
    { }
}
