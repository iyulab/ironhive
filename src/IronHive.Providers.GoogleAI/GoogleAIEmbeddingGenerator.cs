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
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
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

                return batch.Zip(res.Embeddings ?? [], (b, e) => new EmbeddingResult
                {
                    Index = b.index,
                    Embedding = e.Values?.Select(v => (float)v).ToArray()
                });
            });

        var batchResults = await Task.WhenAll(batchTasks);
        return batchResults.SelectMany(r => r);
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
