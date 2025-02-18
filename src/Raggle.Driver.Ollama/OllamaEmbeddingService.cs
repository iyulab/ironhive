using Raggle.Abstractions.AI;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.Ollama.Embeddings;

namespace Raggle.Driver.Ollama;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly OllamaEmbeddingClient _client;

    public OllamaEmbeddingService(OllamaConfig? config = null)
    {
        _client = new OllamaEmbeddingClient(config);
    }

    public OllamaEmbeddingService(string endPoint)
    {
        _client = new OllamaEmbeddingClient(endPoint);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new EmbeddingModel
        {
            Model = m.Name,
            Owner = null,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = model,
            Input = [input]
        }, cancellationToken);

        return response.Embeddings.FirstOrDefault()
            ?? throw new InvalidOperationException("Failed to generate embedding");
    }

    /// <inheritdoc />
    public async Task<EmbeddingsResponse> EmbedBatchAsync(
        EmbeddingsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = request.Model,
            Input = request.Input
        }, cancellationToken);

        var embeddings = response.Embeddings.Select((e, i) => new EmbeddingsResponse.EmbeddingData
        {
            Index = i,
            Embedding = e
        });

        return new EmbeddingsResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
        };
    }
}
