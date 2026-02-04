using IronHive.Core.Compatibility;
using Microsoft.Extensions.AI;

namespace IronHive.Abstractions.Messages;

public static class MessageGeneratorExtensions
{
    /// <summary>
    /// IMessageGenerator를 IChatClient로 변환합니다.
    /// </summary>
    /// <param name="generator">IronHive 메시지 생성기</param>
    /// <param name="modelId">사용할 모델 ID</param>
    /// <param name="providerName">Provider 이름 (선택)</param>
    /// <returns>IChatClient 인스턴스</returns>
    public static IChatClient AsChatClient(
        this IMessageGenerator generator,
        string modelId,
        string? providerName = null)
    {
        return new ChatClientAdapter(generator, modelId, providerName);
    }
}
