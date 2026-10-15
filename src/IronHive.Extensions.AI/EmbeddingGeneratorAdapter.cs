using Microsoft.Extensions.AI;
using IronHiveEmbedding = IronHive.Abstractions.Embedding;

namespace IronHive.Extensions.AI;

/// <summary>
/// IronHive IEmbeddingGenerator를 Microsoft.Extensions.AI IEmbeddingGenerator로 래핑하는 어댑터입니다.
/// </summary>
public class EmbeddingGeneratorAdapter : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IronHiveEmbedding.IEmbeddingGenerator _generator;
    private readonly string _modelId;
    private readonly string _providerName;
    private readonly int? _dimensions;

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
        : this(generator, modelId, providerName, dimensions: null)
    {
    }

    /// <summary>
    /// Creates the adapter for a model whose vector size the caller knows. The size is reported as
    /// <see cref="EmbeddingGeneratorMetadata.DefaultModelDimensions"/> - IronHive's generator interface does not expose
    /// one, and consumers that size a store or check vectors (vector indexes, semantic chunkers) read it from there -
    /// and every vector the model returns is checked against it.
    /// </summary>
    /// <param name="generator">The IronHive embedding generator.</param>
    /// <param name="modelId">The model to embed with.</param>
    /// <param name="providerName">The provider name to report; "IronHive" when null.</param>
    /// <param name="dimensions">The model's vector size; null when unknown.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dimensions"/> is not positive.</exception>
    public EmbeddingGeneratorAdapter(
        IronHiveEmbedding.IEmbeddingGenerator generator,
        string modelId,
        string? providerName,
        int? dimensions)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _providerName = providerName ?? "IronHive";
        if (dimensions is <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimensions), dimensions, "The vector size must be positive.");
        _dimensions = dimensions;
    }

    /// <summary>
    /// The provider name, default model and (when given) vector size this generator reports — also what
    /// <see cref="GetService"/> answers for <see cref="EmbeddingGeneratorMetadata"/>, which is where M.E.AI middleware
    /// (e.g. OpenTelemetry) and consumers that size a vector store look for it.
    /// </summary>
    public EmbeddingGeneratorMetadata Metadata => new(
        providerName: _providerName,
        providerUri: null,
        defaultModelId: _modelId,
        defaultModelDimensions: _dimensions);

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputList = values.ToList();
        var modelId = options?.ModelId ?? _modelId;

        var response = await _generator.EmbedBatchAsync(modelId, inputList, cancellationToken)
            .ConfigureAwait(false);

        var embeddings = response.Results
            .Where(r => r.Embedding != null)
            .Select(r => new Embedding<float>(r.Embedding!) { ModelId = response.Model ?? modelId })
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

        // The reported size is a promise to whoever sized a store from it: a model that answers with another size is
        // a misconfiguration to surface here, not a vector to hand on.
        if (_dimensions is int declared && options?.Dimensions is null && embeddings.Count > 0
            && embeddings[0].Vector.Length != declared)
        {
            throw new InvalidOperationException(
                $"Model '{modelId}' produced vectors of {embeddings[0].Vector.Length} dimension(s), but this generator " +
                $"reports {declared}. Pass the model's actual size to AsEmbeddingGenerator.");
        }

        // Usage carries only what the provider reported. When it reports nothing, Usage stays unset rather than
        // being estimated: the number of input strings is not a token count, and a consumer feeding Usage into
        // cost or budget arithmetic is better served by "unknown" than by a confident wrong number.
        var generated = new GeneratedEmbeddings<Embedding<float>>(embeddings);
        if (response.InputTokens is int inputTokens)
            generated.Usage = new UsageDetails { InputTokenCount = inputTokens, TotalTokenCount = inputTokens };
        return generated;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        if (serviceKey is not null)
            return null;

        if (serviceType == typeof(EmbeddingGeneratorMetadata))
            return Metadata;
        if (serviceType == typeof(IronHiveEmbedding.IEmbeddingGenerator))
            return _generator;
        if (serviceType.IsInstanceOfType(this))
            return this;

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
