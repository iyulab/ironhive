using Raggle.Abstractions.Engines;
using Raggle.Engines.OpenAI.Configurations;
using Raggle.Engines.OpenAI.Embeddings;

namespace Raggle.Engines.OpenAI;

public class OpenAIEmbeddingEngine : IEmbeddingEngine
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
