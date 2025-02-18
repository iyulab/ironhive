using Raggle.Abstractions.AI;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Driver.OpenAI.Embeddings;
using Raggle.Driver.OpenAI.Extensions;

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
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Where(m => m.IsEmbedding())
                    .Select(m => new EmbeddingModel
                    {
                        Model = m.ID,
                        Owner = m.OwnedBy,
                        CreatedAt = m.Created,
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
            Input = [input],
        }, cancellationToken);

        return response.First().Embedding
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
            Input = request.Input,
        }, cancellationToken);

        var embeddings = response.Select(r => new EmbeddingsResponse.EmbeddingData
        {
            Index = r.Index,
            Embedding = r.Embedding,
        });

        return new EmbeddingsResponse
        {
            Model = request.Model,
            Embeddings = embeddings
        };
    }
}
