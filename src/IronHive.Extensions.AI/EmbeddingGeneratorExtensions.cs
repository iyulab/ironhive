using Microsoft.Extensions.AI;
using IHiveEmbeddingGenerator = IronHive.Abstractions.Embedding.IEmbeddingGenerator;

namespace IronHive.Extensions.AI;

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
        this IHiveEmbeddingGenerator generator,
        string modelId,
        string? providerName = null)
    {
        return new EmbeddingGeneratorAdapter(generator, modelId, providerName);
    }

    /// <summary>
    /// Converts an IronHive embedding generator into a <c>Microsoft.Extensions.AI</c> one that also reports the model's
    /// vector size (<see cref="EmbeddingGeneratorMetadata.DefaultModelDimensions"/>), so consumers that size a store or
    /// check vectors from the metadata need no separate dimension setting.
    /// </summary>
    /// <param name="generator">The IronHive embedding generator.</param>
    /// <param name="modelId">The model to embed with.</param>
    /// <param name="dimensions">The model's vector size; every returned vector is checked against it.</param>
    /// <param name="providerName">The provider name to report; "IronHive" when null.</param>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(
        this IHiveEmbeddingGenerator generator,
        string modelId,
        int dimensions,
        string? providerName = null)
    {
        return new EmbeddingGeneratorAdapter(generator, modelId, providerName, dimensions);
    }
}
