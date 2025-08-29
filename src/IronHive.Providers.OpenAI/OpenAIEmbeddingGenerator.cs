using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Embedding;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI;

public class OpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIEmbeddingClient _client;
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIEmbeddingGenerator(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingGenerator(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new OpenAIEmbeddingRequest
        {
            Model = modelId,
            Input = [ input ]
        }, cancellationToken);

        return res.Data?.FirstOrDefault()?.Embedding ??
                throw new InvalidOperationException("No embedding found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        // OpenAI 임베딩 모델은 최대 300,000 토큰까지 배치 처리를 지원하므로, 나누어 배치 처리를 수행합니다.
        const int MaxTokensPerBatch = 300_000;
        var tokens = await CountTokensBatchAsync(modelId, inputs, cancellationToken);
        var chunks = tokens.ChunkBy(MaxTokensPerBatch);

        var tasks = chunks.Select(async chunk =>
        {
            var inputs = chunk.Select(x => x.Text).ToList();
            var indices = chunk.Select(x => x.Index).ToList();

            var res = await _client.PostEmbeddingAsync(new OpenAIEmbeddingRequest
            {
                Model = modelId,
                Input = inputs
            }, cancellationToken);

            return res.Data?.Select(r => new EmbeddingResult
            {
                Index = r.Index.HasValue ? indices.ElementAt(r.Index.Value) : null,
                Embedding = r.Embedding,
            }) ?? [];
        });

        var results = (await Task.WhenAll(tasks))
            .SelectMany(r => r)
            .OrderBy(r => r.Index)
            .ToList();

        return results;
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        // OpenAI Embedding models use the cl100k_base tokenizer
        // see https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken

        var count = _tokenizer.CountTokens(input);
        return await Task.FromResult(count);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
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

        // 생성자에서 인덱스 순으로 정렬을 보장합니다.
        return [.. items];
    }
}
