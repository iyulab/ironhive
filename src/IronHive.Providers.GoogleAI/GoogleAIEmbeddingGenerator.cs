using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Embedding;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly Client _client;

    public GoogleAIEmbeddingGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIEmbeddingGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
    }

    public GoogleAIEmbeddingGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.Models.EmbedContentAsync(modelId, input, cancellationToken: cancellationToken);
        var embedding = res.Embeddings?.FirstOrDefault()
            ?? throw new InvalidOperationException("No embedding found in response.");

        return embedding.Values?.Select(v => (float)v).ToArray()
            ?? throw new InvalidOperationException("No embedding values found in response.");
    }

    /// <summary>
    /// Google AI의 embedContent 요청 하나당 허용되는 최대 입력 개수입니다.
    /// </summary>
    private const int MaxBatchSize = 100;

    /// <inheritdoc />
    public async Task<EmbeddingResponse> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var indexed = inputs.Select((input, index) => (input, index)).ToList();

        var batchTasks = indexed
            .Chunk(MaxBatchSize)
            .Select(async batch =>
            {
                var contents = batch.Select(b => new Content
                {
                    Parts = [new Part { Text = b.input }]
                }).ToList();

                var res = await _client.Models.EmbedContentAsync(modelId, contents, cancellationToken: cancellationToken);
                var embeddings = res.Embeddings ?? [];

                var results = batch.Zip(embeddings, (b, e) => new EmbeddingResult
                {
                    Index = b.index,
                    Embedding = e.Values?.Select(v => (float)v).ToArray()
                }).ToList();

                // Vertex AI reports a token count per embedding; the Gemini Developer API reports none.
                int? tokens = embeddings.Count > 0 && embeddings.All(e => e.Statistics?.TokenCount != null)
                    ? (int)embeddings.Sum(e => e.Statistics!.TokenCount!.Value)
                    : null;
                return (Results: results, InputTokens: tokens);
            });

        var parts = await Task.WhenAll(batchTasks);
        return new EmbeddingResponse
        {
            Results = parts.SelectMany(p => p.Results).ToList(),
            InputTokens = parts.All(p => p.InputTokens.HasValue) ? parts.Sum(p => p.InputTokens!.Value) : null,
        };
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.Models.CountTokensAsync(modelId, input, cancellationToken: cancellationToken);
        return res.TotalTokens
            ?? throw new InvalidOperationException("No token count found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var inputList = inputs.ToList();
        var results = new List<EmbeddingTokens>(inputList.Count);

        for (int i = 0; i < inputList.Count; i++)
        {
            var res = await _client.Models.CountTokensAsync(modelId, inputList[i], cancellationToken: cancellationToken);
            results.Add(new EmbeddingTokens
            {
                Index = i,
                Text = inputList[i],
                TokenCount = res.TotalTokens
                    ?? throw new InvalidOperationException($"No token count found for the input[{i}].")
            });
        }

        return results;
    }
}
