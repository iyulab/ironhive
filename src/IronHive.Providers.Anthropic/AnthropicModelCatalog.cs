using IronHive.Abstractions.Catalog;
using IronHive.Providers.Anthropic.Clients;
using IronHive.Providers.Anthropic.Payloads.Models;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicModelCatalog : IModelCatalog
{
    private readonly AnthropicModelsClient _client;

    public AnthropicModelCatalog(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicModelCatalog(AnthropicConfig config)
    {
        _client = new AnthropicModelsClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var req = new ListModelsRequest
        {
            Limit = 999,
        };
        var res = await _client.GetModelsAsync(req, cancellationToken);

        return res.Data.Select(m => new GenericModelSpec
        {
            ModelId = m.Id,
            DisplayName = m.DisplayName,
            CreatedAt = m.CreatedAt,
        });
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);

        return model is not null
            ? new GenericModelSpec
            {
                ModelId = model.Id,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt,
            }
            : null;
    }
}