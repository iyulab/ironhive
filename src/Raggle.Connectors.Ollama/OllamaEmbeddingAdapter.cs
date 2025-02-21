using Raggle.Abstractions.Embedding;
using Raggle.Connectors.Ollama.Configurations;
using Raggle.Connectors.Ollama.Embeddings;

namespace Raggle.Connectors.Ollama;

public class OllamaEmbeddingAdapter : IEmbeddingAdapter
{
    private readonly OllamaEmbeddingClient _client;

    public OllamaEmbeddingAdapter(OllamaConfig? config = null)
    {
        _client = new OllamaEmbeddingClient(config);
    }

    public OllamaEmbeddingAdapter(string baseUrl)
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
            Owner = null,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
        });
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
