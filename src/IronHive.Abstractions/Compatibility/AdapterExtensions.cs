using Microsoft.Extensions.AI;
using IronHive.Abstractions.Messages;
using IronHiveEmbedding = IronHive.Abstractions.Embedding;

namespace IronHive.Abstractions.Compatibility;

/// <summary>
/// Microsoft.Extensions.AI 호환성을 위한 확장 메서드를 제공합니다.
/// </summary>
public static class AdapterExtensions
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

    /// <summary>
    /// IEmbeddingGenerator를 Microsoft.Extensions.AI IEmbeddingGenerator로 변환합니다.
    /// </summary>
    /// <param name="generator">IronHive 임베딩 생성기</param>
    /// <param name="modelId">사용할 모델 ID</param>
    /// <param name="providerName">Provider 이름 (선택)</param>
    /// <returns>IEmbeddingGenerator 인스턴스</returns>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(
        this IronHiveEmbedding.IEmbeddingGenerator generator,
        string modelId,
        string? providerName = null)
    {
        return new EmbeddingGeneratorAdapter(generator, modelId, providerName);
    }
}
