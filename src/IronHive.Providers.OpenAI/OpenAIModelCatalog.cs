using IronHive.Abstractions.Catalog;
using OpenAI.Models;

namespace IronHive.Providers.OpenAI;

public class OpenAIModelCatalog : IModelCatalog
{
    private readonly OpenAIModelClient _client;

    public OpenAIModelCatalog(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIModelCatalog(OpenAIConfig config)
    {
        _client = OpenAIClientFactory.Create(config).GetOpenAIModelClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _client.GetModelsAsync(cancellationToken);
        return result.Value
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new GenericModelSpec
            {
                ModelId = m.Id,
                DisplayName = m.Id,
                OwnedBy = m.OwnedBy,
                CreatedAt = m.CreatedAt.UtcDateTime,
            });
    }

    /// <inheritdoc />
    public virtual async Task<IModelSpec?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.GetModelAsync(modelId, cancellationToken);
            var model = result.Value;
            return new GenericModelSpec
            {
                ModelId = model.Id,
                DisplayName = model.Id,
                OwnedBy = model.OwnedBy,
                CreatedAt = model.CreatedAt.UtcDateTime,
            };
        }
        catch
        {
            return null;
        }
    }
}
