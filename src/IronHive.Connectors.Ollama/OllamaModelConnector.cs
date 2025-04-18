using IronHive.Abstractions.Models;
using IronHive.Connectors.Ollama.Clients;

namespace IronHive.Connectors.Ollama;

public class OllamaModelConnector : IModelConnector
{
    private readonly OllamaModelClient _client;

    public OllamaModelConnector(OllamaConfig? config = null)
    {
        _client = new OllamaModelClient(config);
    }

    public OllamaModelConnector(string baseUrl)
    {
        _client = new OllamaModelClient(baseUrl);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new ModelDescriptor
        {
            Id = m.Name,
            CreatedAt = m.ModifiedAt,
        })
        .OrderByDescending(m => m.CreatedAt)
        .ToArray();
    }

    /// <inheritdoc />
    public Task<ModelDescriptor?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        //var model = await _client.GetModelAsync(modelId, cancellationToken);
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return Task.FromResult<ModelDescriptor?>(null);
        }
        else
        {
            return Task.FromResult<ModelDescriptor?>(new ModelDescriptor
            {
                Id = modelId
            });
        }
    }
}
