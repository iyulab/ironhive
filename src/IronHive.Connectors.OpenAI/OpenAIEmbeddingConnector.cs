using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Json;
using IronHive.Connectors.OpenAI.Embeddings;
using System.Reflection;

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
        if (_client.Client.BaseAddress?.ToString() == OpenAIConstants.DefaultBaseUrl)
        {
            // OpenAI 모델을 호출하는 경우 내장 리소스를 사용
            var assembly = Assembly.GetExecutingAssembly();
            var resource = await JsonResourceLoader.LoadAsync<IEnumerable<EmbeddingModel>>(
                assembly: assembly,
                resourceName: $"{assembly.GetName().Name}.Resources.OpenAIEmbeddingModels.json",
                options: _client.JsonOptions,
                cancellationToken: cancellationToken);
            if (resource.Data == null)
                throw new InvalidOperationException("Failed to load OpenAI models.");

            return resource.Data;
        }
        else
        {
            // 다른 OpenAI 서버를 호출하는 경우 API를 사용
            var models = await _client.GetModelsAsync(cancellationToken);
            return models.Where(m => m.IsEmbedding())
                        .Select(m => new EmbeddingModel
                        {
                            Model = m.Id,
                            CreatedAt = m.Created,
                        });
        }
    }

    /// <inheritdoc />
    public async Task<EmbeddingModel> GetModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var models = await GetModelsAsync(cancellationToken);
        return models.First(m => m.Model == model);
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
