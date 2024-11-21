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
        var ollamaModels = await _client.GetEmbeddingModelsAsync(cancellationToken);
        return ollamaModels.Select(m => new EmbeddingModel
        {
            Model = m.Name,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
            Owner = null
        });
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
            Input = [input]
        };
        var embedding = (await EmbeddingsAsync(request, cancellationToken).ConfigureAwait(false))
                        .FirstOrDefault()
                        ?? throw new InvalidOperationException("Failed to get embedding");
        return embedding;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Abstractions.AI.EmbeddingResponse>> EmbeddingsAsync(
        Abstractions.AI.EmbeddingRequest request, 
        CancellationToken cancellationToken = default)
    {
        var ollamaRequest = new Embeddings.Models.EmbeddingRequest
        {
            Model = request.Model,
            Input = request.Input
        };
        var response = await _client.PostEmbeddingAsync(ollamaRequest, cancellationToken);
        var embeddings = new List<Abstractions.AI.EmbeddingResponse>();
        for (var i = 0; i < response.Embeddings.Length; i++)
        {
            var embedding = response.Embeddings[i];
            embeddings.Add(new Abstractions.AI.EmbeddingResponse
            {
                Index = i,
                Embedding = embedding
            });
        }
        return embeddings;
    }
}
