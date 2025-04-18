using IronHive.Abstractions.Models;
using IronHive.Connectors.Anthropic.Clients;

namespace IronHive.Connectors.Anthropic;

public class AnthropicModelConnector : IModelConnector
{
    private readonly AnthropicModelClient _client;

    public AnthropicModelConnector(AnthropicConfig config)
    {
        _client = new AnthropicModelClient(config);
    }

    public AnthropicModelConnector(string apiKey)
    {
        _client = new AnthropicModelClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);

        return models.Select(m => new ModelDescriptor
        {
            Id = m.Id,
            Display = m.DisplayName,
            CreatedAt = m.CreatedAt,
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
                Display = model.DisplayName,
                CreatedAt = model.CreatedAt,
            }
            : null;
    }
}
