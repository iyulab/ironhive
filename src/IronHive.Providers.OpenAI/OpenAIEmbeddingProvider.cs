using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Embedding;

namespace IronHive.Providers.OpenAI;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly OpenAIEmbeddingClient _client;

    public OpenAIEmbeddingProvider(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingProvider(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = modelId,
            Input = new[] { input }
        }, cancellationToken);

        return res.FirstOrDefault()?.Embedding ??
                throw new InvalidOperationException("No embedding found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = modelId,
            Input = inputs,
        }, cancellationToken);

        var result = res.Select(r => new EmbeddingResult
        {
            Index = r.Index,
            Embedding = r.Embedding,
        });
        return result;
    }
}
