using IronHive.Abstractions.Models;
using IronHive.Connectors.OpenAI.Clients;

namespace IronHive.Connectors.OpenAI;

public class OpenAIModelConnector : IModelConnector
{
    private readonly OpenAIModelClient _client;

    public OpenAIModelConnector(OpenAIConfig config)
    {
        _client = new OpenAIModelClient(config);
    }

    public OpenAIModelConnector(string apiKey)
    {
        _client = new OpenAIModelClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetListModelsAsync(cancellationToken);

        return models.Select(m => new ModelDescriptor
        {
            Id = m.Id,
            Owner = m.OwnedBy,
            CreatedAt = m.Created,
        })
        .OrderByDescending(m => m.CreatedAt)
        .ToArray();
    }

    /// <inheritdoc />
    public async Task<ModelDescriptor?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);

        return model is not null
            ? new ModelDescriptor
            {
                Id = model.Id,
                Owner = model.OwnedBy,
                CreatedAt = model.Created,
            }
            : null;
    }
}
