using IronHive.Abstractions.Catalog;
using IronHive.Providers.Anthropic.Catalog;

namespace IronHive.Providers.Anthropic;

public class AnthropicModelCatalog : IModelCatalog
{
    private readonly AnthropicModelsClient _client;

    public AnthropicModelCatalog(AnthropicConfig config)
    {
        _client = new AnthropicModelsClient(config);
    }

    public AnthropicModelCatalog(string apiKey)
    {
        _client = new AnthropicModelsClient(apiKey);
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
        var req = new AnthropicListModelsRequest
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
