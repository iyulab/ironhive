using Anthropic;
using Anthropic.Models.Models;
using IronHive.Abstractions.Models;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicModelFinder : IModelFinder
{
    private readonly IAnthropicClient _client;

    public AnthropicModelFinder(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicModelFinder(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelCard>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var page = await _client.Models.List(new ModelListParams
        {
            Limit = 1000,
        }, cancellationToken);

        var models = new List<IModelCard>();
        await foreach (var model in page.Paginate(cancellationToken))
        {
            models.Add(new ModelCard
            {
                ModelId = model.ID,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt.UtcDateTime,
            });
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<IModelCard?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _client.Models.Retrieve(new ModelRetrieveParams
            {
                ModelID = modelId
            }, cancellationToken);
            
            return new ModelCard
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
