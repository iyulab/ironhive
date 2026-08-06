using Microsoft.Extensions.AI;
using IronHiveEmbedding = IronHive.Abstractions.Embedding;

namespace IronHive.Core.Microsoft;

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

        // GeneratedEmbeddings is positional: the caller matches result[i] to input[i]. Dropping a
        // failed embedding would therefore not lose one result, it would shift every later result
        // onto the wrong input -- and silently, since a short list is still a valid list. Refuse
        // to return a set the caller cannot align rather than let it associate vectors with the
        // wrong text.
        if (embeddings.Count != inputList.Count)
        {
            throw new InvalidOperationException(
                $"The embedding provider returned {embeddings.Count} embedding(s) for {inputList.Count} " +
                $"input(s) using model '{modelId}'. Results are positional, so a partial set cannot be " +
                "matched to its inputs.");
        }

        // Dimensions is documented as honored "if supported", so rejecting it up front would be
        // wrong -- the request may well be satisfied by how the model or deployment is configured.
        // What must not happen is the caller asking for a size, receiving another, and being told
        // nothing: the vectors would then be silently incompatible with a store provisioned for the
        // requested size. So the request is checked against the result, not against a capability list.
        if (options?.Dimensions is int requested && embeddings.Count > 0)
        {
            var actual = embeddings[0].Vector.Length;
            if (actual != requested)
            {
                throw new InvalidOperationException(
                    $"{requested} dimension(s) were requested, but model '{modelId}' produced vectors of " +
                    $"{actual}. This provider cannot resize embeddings; request a model or deployment that " +
                    "emits the required size, or leave Dimensions unset to accept the model's native size.");
            }
        }

        // Usage is deliberately left unset. EmbeddingResult carries no token counts, and the number
        // of input strings is not an approximation of a token count -- it is a different quantity,
        // wrong by whatever the average input length happens to be. A consumer feeding Usage into
        // cost or budget arithmetic is better served by "unknown" than by a confident wrong number.
        return new GeneratedEmbeddings<Embedding<float>>(embeddings);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
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
