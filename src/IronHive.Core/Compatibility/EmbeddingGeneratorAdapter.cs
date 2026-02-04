using Microsoft.Extensions.AI;
using IronHiveEmbedding = IronHive.Abstractions.Embedding;

namespace IronHive.Core.Compatibility;

/// <summary>
/// IronHive IEmbeddingGenerator를 Microsoft.Extensions.AI IEmbeddingGenerator로 래핑하는 어댑터입니다.
/// </summary>
public class EmbeddingGeneratorAdapter : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IronHiveEmbedding.IEmbeddingGenerator _generator;
    private readonly string _modelId;
    private readonly string _providerName;

    /// <summary>
    /// EmbeddingGeneratorAdapter의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="generator">IronHive 임베딩 생성기</param>
    /// <param name="modelId">사용할 모델 ID</param>
    /// <param name="providerName">Provider 이름 (선택)</param>
    public EmbeddingGeneratorAdapter(
        IronHiveEmbedding.IEmbeddingGenerator generator,
        string modelId,
        string? providerName = null)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerName = providerName ?? "IronHive";
    }

    /// <inheritdoc />
    public EmbeddingGeneratorMetadata Metadata => new(
        providerName: _providerName,
        providerUri: null,
        defaultModelId: _modelId);

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputList = values.ToList();
        var modelId = options?.ModelId ?? _modelId;

        var results = await _generator.EmbedBatchAsync(modelId, inputList, cancellationToken)
            .ConfigureAwait(false);

        var embeddings = results
            .Where(r => r.Embedding != null)
            .Select(r => new Embedding<float>(r.Embedding!))
            .ToList();

        return new GeneratedEmbeddings<Embedding<float>>(embeddings)
        {
            Usage = new UsageDetails
            {
                InputTokenCount = inputList.Count,
                TotalTokenCount = inputList.Count
            }
        };
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? key = null)
    {
        if (serviceType == typeof(IronHiveEmbedding.IEmbeddingGenerator))
            return _generator;

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
