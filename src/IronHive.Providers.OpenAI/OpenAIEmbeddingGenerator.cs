using IronHive.Abstractions.Embedding;
using OpenAI;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI에서 제공하는 임베딩 모델을 사용하여 텍스트 임베딩을 생성하는 클래스입니다.
/// </summary>
public class OpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIClient _openai;
    // https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIEmbeddingGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIEmbeddingGenerator(OpenAIConfig config)
    {
        _openai = OpenAIClientFactory.Create(config);
    }

    // An embedding request this provider cannot extend: refusing is the only answer that does not hand the caller a
    // request other than the one asked for.
    private static void RejectUnsupported(EmbeddingRequestOptions? options)
    {
        if (options?.ExtraBody is { Count: > 0 })
            throw new NotSupportedException(
                "EmbeddingRequestOptions.ExtraBody is not supported by the OpenAI provider; the OpenAI-compatible provider honours it.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual Task<float[]> EmbedAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
        => EmbedAsync(modelId, input, options: null, cancellationToken);

    /// <inheritdoc />
    public virtual async Task<float[]> EmbedAsync(
        string modelId,
        string input,
        EmbeddingRequestOptions? options,
        CancellationToken cancellationToken = default)
    {
        RejectUnsupported(options);
        var client = _openai.GetEmbeddingClient(modelId);
        var result = await client.GenerateEmbeddingAsync(input, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }

    /// <inheritdoc />
    public virtual Task<EmbeddingResponse> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
        => EmbedBatchAsync(modelId, inputs, options: null, cancellationToken);

    /// <inheritdoc />
    public virtual async Task<EmbeddingResponse> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        EmbeddingRequestOptions? options,
        CancellationToken cancellationToken = default)
    {
        RejectUnsupported(options);
        // https://platform.openai.com/docs/api-reference/embeddings/create#embeddings-create-input
        const int MaxTokensPerBatch = 300_000;
        var tokens = await CountTokensBatchAsync(modelId, inputs, cancellationToken);
        var chunks = tokens.ChunkBy(MaxTokensPerBatch);

        var client = _openai.GetEmbeddingClient(modelId);

        var tasks = chunks.Select(async chunk =>
        {
            var texts = chunk.Select(x => x.Text).ToList();
            var indices = chunk.Select(x => x.Index).ToList();

            var result = await client.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);
            var collection = result.Value;

            var results = collection.Select((e, i) => new EmbeddingResult
            {
                Index = indices.ElementAt(i),
                Embedding = e.ToFloats().ToArray(),
            }).ToList();

            // A server that omits "usage" deserializes to an absent object or a zero count; neither is a report.
            var reported = collection.Usage is { } usage && usage.InputTokenCount > 0 ? usage.InputTokenCount : (int?)null;
            return (Results: results, InputTokens: reported, Model: string.IsNullOrEmpty(collection.Model) ? null : collection.Model);
        });

        var parts = await Task.WhenAll(tasks);
        return new EmbeddingResponse
        {
            Results = parts.SelectMany(p => p.Results).OrderBy(r => r.Index).ToList(),
            // Summed only when every request reported: a partial sum would read as the whole call.
            InputTokens = parts.All(p => p.InputTokens.HasValue) ? parts.Sum(p => p.InputTokens!.Value) : null,
            Model = parts.Select(p => p.Model).FirstOrDefault(m => m != null),
        };
    }

    /// <inheritdoc />
    public virtual async Task<int> CountTokensAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var count = _tokenizer.CountTokens(input);
        return await Task.FromResult(count);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken)
    {
        var items = new EmbeddingTokens[inputs.Count()];

        await Parallel.ForEachAsync(Enumerable.Range(0, inputs.Count()), cancellationToken, async (idx, ct) =>
        {
            await Task.Run(() =>
            {
                var text = inputs.ElementAt(idx);
                var tokenCount = _tokenizer.CountTokens(text);
                items[idx] = new EmbeddingTokens
                {
                    Index = idx,
                    Text = text,
                    TokenCount = tokenCount
                };
            }, ct);
        });

        return [.. items];
    }
}
