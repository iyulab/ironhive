using System.Runtime.CompilerServices;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// OpenAI 호환 서비스를 위한 기본 메시지 생성기입니다.
/// OpenAI Client를 직접 사용하며, 요청/응답 변환을 수행합니다.
/// </summary>
internal class CompatibleChatMessageGenerator : OpenAIChatMessageGenerator
{
    protected readonly CompatibleConfig _config;

    public CompatibleChatMessageGenerator(CompatibleConfig config)
        : base(config.ToOpenAI())
    {
        _config = config;
    }
}
