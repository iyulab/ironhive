using IronHive.Core.Compatibility;
using Microsoft.Extensions.AI;

namespace IronHive.Abstractions.Embedding;

public static class EmbeddingGeneratorExtensions
{
    /// <summary>
    /// IEmbeddingGenerator를 Microsoft.Extensions.AI IEmbeddingGenerator로 변환합니다.
    /// </summary>
    /// <param name="generator">IronHive 임베딩 생성기</param>
    /// <param name="modelId">사용할 모델 ID</param>
    /// <param name="providerName">Provider 이름 (선택)</param>
    /// <returns>IEmbeddingGenerator 인스턴스</returns>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(
        this IEmbeddingGenerator generator,
        string modelId,
        string? providerName = null)
    {
        return new EmbeddingGeneratorAdapter(generator, modelId, providerName);
    }
}
