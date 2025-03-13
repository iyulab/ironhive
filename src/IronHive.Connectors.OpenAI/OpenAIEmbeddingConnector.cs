using IronHive.Abstractions.Embedding;
using IronHive.Connectors.OpenAI.Configurations;
using IronHive.Connectors.OpenAI.Embeddings;
using IronHive.Connectors.OpenAI.Extensions;

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
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Where(m => m.IsEmbedding())
                    .Select(m => new EmbeddingModel
                    {
                        Model = m.Id,
                        Owner = m.OwnedBy,
                        CreatedAt = m.Created,
                    });
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
