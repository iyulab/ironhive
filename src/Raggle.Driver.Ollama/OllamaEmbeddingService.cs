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
    public async Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetEmbeddingModelsAsync(cancellationToken);
        return models.Select(m => new EmbeddingModel
        {
            Model = m.Name,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
            Owner = null
        });
    }

    /// <inheritdoc />
    public async Task<EmbeddingResponse> EmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var _request = new Embeddings.Models.EmbeddingRequest
        {
            Model = request.Model,
            Input = [ request.Input ]
        };
        var response = await _client.PostEmbeddingAsync(_request, cancellationToken);
        
        return new EmbeddingResponse
        {
            Model = response.Model,
            Embedding = response.Embeddings.FirstOrDefault(),
            TimeStamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<EmbeddingsResponse> EmbeddingsAsync(
        EmbeddingsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var _request = new Embeddings.Models.EmbeddingRequest
        {
            Model = request.Model,
            Input = request.Input
        };
        var response = await _client.PostEmbeddingAsync(_request, cancellationToken);
        var embeddings = response.Embeddings.Select((e, i) => new EmbeddingsResponse.EmbeddingData
        {
            Index = i,
            Embedding = e
        }).ToArray();

        return new EmbeddingsResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            TimeStamp = DateTime.UtcNow
        };
    }
}
