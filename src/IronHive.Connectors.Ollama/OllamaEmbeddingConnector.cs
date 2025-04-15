using IronHive.Abstractions.Embedding;
using IronHive.Connectors.Ollama.Embeddings;

namespace IronHive.Connectors.Ollama;

public class OllamaEmbeddingConnector : IEmbeddingConnector
{
    private readonly OllamaEmbeddingClient _client;

    public OllamaEmbeddingConnector(OllamaConfig? config = null)
    {
        _client = new OllamaEmbeddingClient(config);
    }

    public OllamaEmbeddingConnector(string baseUrl)
    {
        _client = new OllamaEmbeddingClient(baseUrl);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new EmbeddingModel
        {
            Model = m.Name,
            CreatedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<EmbeddingModel> GetModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var models = await GetModelsAsync(cancellationToken);
        return models.First(m => m.Model == model);
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

        return res.Embeddings.FirstOrDefault() ??
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
            Input = inputs
        }, cancellationToken);

        var result = res.Embeddings.Select((e, i) => new EmbeddingResult
        {
            Index = i,
            Embedding = e
        });
        return result;
    }
}
