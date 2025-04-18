using IronHive.Abstractions.Embedding;
using IronHive.Connectors.OpenAI.Clients;
using IronHive.Connectors.OpenAI.Embeddings;

namespace IronHive.Connectors.OpenAI;

public class OpenAIEmbeddingConnector : IEmbeddingConnector
{
    private readonly OpenAIEmbeddingClient _client;

    public OpenAIEmbeddingConnector(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingConnector(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string model, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = model,
            Input = new[] { input }
        }, cancellationToken);

        return res.FirstOrDefault()?.Embedding ??
                throw new InvalidOperationException("No embedding found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = model,
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
