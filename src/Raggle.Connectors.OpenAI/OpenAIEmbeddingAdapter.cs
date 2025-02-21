using Raggle.Abstractions.Embedding;
using Raggle.Connectors.OpenAI.Configurations;
using Raggle.Connectors.OpenAI.Embeddings;
using Raggle.Connectors.OpenAI.Extensions;

namespace Raggle.Connectors.OpenAI;

public class OpenAIEmbeddingAdapter : IEmbeddingAdapter
{
    private readonly OpenAIEmbeddingClient _client;

    public OpenAIEmbeddingAdapter(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingAdapter(string apiKey)
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
