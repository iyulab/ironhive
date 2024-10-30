using Raggle.Abstractions.AI;
using Raggle.Connector.OpenAI.Configurations;
using Raggle.Connector.OpenAI.Embeddings;
using Raggle.Connector.OpenAI.Embeddings.Models;

namespace Raggle.Connector.OpenAI;

public class OpenAIEmbeddingEngine : IEmbeddingService
{
    private readonly OpenAIEmbeddingClient _client;

    public OpenAIEmbeddingEngine(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingEngine(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    public async Task<EmbeddingModel[]> GetEmbeddingModelsAsync()
    {
        var models = await _client.GetEmbeddingModelsAsync();
        return models.Select(m => new EmbeddingModel
        {
            ModelID = m.ID,
            CreatedAt = m.Created,
            Owner = "OpenAI"
        }).ToArray();
    }

    public async Task<float[]> EmbeddingAsync(string input, EmbeddingOptions options)
    {
        var request = new EmbeddingRequest
        {
            Model = options.ModelId,
            Input = [input]
        };
        var response = await _client.PostEmbeddingAsync(request);
        return response.First().Embedding.ToArray();
    }

    public async Task<float[][]> EmbeddingsAsync(ICollection<string> inputs, EmbeddingOptions options)
    {
        var request = new EmbeddingRequest
        {
            Model = options.ModelId,
            Input = inputs.ToArray(),
        };
        var response = await _client.PostEmbeddingAsync(request);
        return response.Select(r => r.Embedding.ToArray()).ToArray();
    }
}
