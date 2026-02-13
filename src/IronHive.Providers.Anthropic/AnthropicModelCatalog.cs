using Anthropic;
using Anthropic.Models.Models;
using IronHive.Abstractions.Catalog;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicModelCatalog : IModelCatalog
{
    private readonly IAnthropicClient _client;

    public AnthropicModelCatalog(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicModelCatalog(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.CreateClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var page = await _client.Models.List(new ModelListParams
        {
            Limit = 1000,
        }, cancellationToken);

        var models = new List<IModelSpec>();
        await foreach (var model in page.Paginate(cancellationToken))
        {
            models.Add(new GenericModelSpec
            {
                ModelId = model.ID,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt.UtcDateTime,
            });
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _client.Models.Retrieve(new ModelRetrieveParams
            {
                ModelID = modelId
            }, cancellationToken);
            return new GenericModelSpec
            {
                ModelId = model.ID,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt.UtcDateTime,
            };
        }
        catch
        {
            return null;
        }
    }
}
