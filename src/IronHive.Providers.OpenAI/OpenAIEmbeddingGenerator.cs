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

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<float[]> EmbedAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var client = _openai.GetEmbeddingClient(modelId);
        var result = await client.GenerateEmbeddingAsync(input, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
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

            return result.Value.Select((e, i) => new EmbeddingResult
            {
                Index = indices.ElementAt(i),
                Embedding = e.ToFloats().ToArray(),
            });
        });

        return (await Task.WhenAll(tasks))
            .SelectMany(r => r)
            .OrderBy(r => r.Index);
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
