using Raggle.Abstractions.AI;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Driver.OpenAI.Embeddings;

namespace Raggle.Driver.OpenAI;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly OpenAIEmbeddingClient _client;

    public OpenAIEmbeddingService(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingService(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetEmbeddingModelsAsync(cancellationToken);
        return models.Select(m => new EmbeddingModel
        {
            Model = m.ID,
            CreatedAt = m.Created,
            Owner = m.OwnedBy,
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task<EmbeddingResponse> EmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var _request = new Embeddings.Models.EmbeddingRequest
        {
            Model = request.Model,
            Input = [ request.Input ],
        };
        var response = await _client.PostEmbeddingAsync(_request, cancellationToken);
        return new EmbeddingResponse
        {
            Model = request.Model,
            Embedding = response.First().Embedding,
            TimeStamp = DateTime.UtcNow,
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
            Input = request.Input,
        };
        var response = await _client.PostEmbeddingAsync(_request, cancellationToken);

        var embeddings = response.Select(r => new EmbeddingsResponse.EmbeddingData
        {
            Index = r.Index,
            Embedding = r.Embedding,
        }).ToArray();

        return new EmbeddingsResponse
        {
            Model = request.Model,
            Embeddings = embeddings,
            TimeStamp = DateTime.UtcNow,
        };
    }
}
