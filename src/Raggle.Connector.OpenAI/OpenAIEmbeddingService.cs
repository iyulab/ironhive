using Raggle.Abstractions.AI;
using Raggle.Connector.OpenAI.Configurations;
using Raggle.Connector.OpenAI.Embeddings;

namespace Raggle.Connector.OpenAI;

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
    public async Task<Abstractions.AI.EmbeddingResponse> EmbeddingAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        var request = new Abstractions.AI.EmbeddingRequest
        {
            Model = model,
            Input = [input],
        };
        var embedding = (await EmbeddingsAsync(request, cancellationToken).ConfigureAwait(false))
                        .FirstOrDefault()
                        ?? throw new InvalidOperationException("Failed to get embedding.");
        return embedding;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Abstractions.AI.EmbeddingResponse>> EmbeddingsAsync(
        Abstractions.AI.EmbeddingRequest request, 
        CancellationToken cancellationToken = default)
    {
        var openaiRequest = new Embeddings.Models.EmbeddingRequest
        {
            Model = request.Model,
            Input = request.Input,
        };

        var response = await _client.PostEmbeddingAsync(openaiRequest, cancellationToken);

        return response
                .Where(r => r.Embedding != null)
                .OrderBy(r => r.Index)
                .Select(r => new Abstractions.AI.EmbeddingResponse
                {
                    Index = r.Index,
                    Embedding = r.Embedding!,
                })
                .ToList();
    }
}
