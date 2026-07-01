using IronHive.Abstractions.Models;
using OpenAI.Models;

namespace IronHive.Providers.OpenAI;

public class OpenAIModelFinder : IModelFinder
{
    private readonly OpenAIModelClient _client;

    public OpenAIModelFinder(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIModelFinder(OpenAIConfig config)
    {
        _client = OpenAIClientFactory.Create(config).GetOpenAIModelClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<IModelCard>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _client.GetModelsAsync(cancellationToken);
        return result.Value
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ModelCard
            {
                ModelId = m.Id,
                DisplayName = m.Id,
                OwnedBy = m.OwnedBy,
                CreatedAt = m.CreatedAt.UtcDateTime,
            });
    }

    /// <inheritdoc />
    public virtual async Task<IModelCard?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.GetModelAsync(modelId, cancellationToken);
            var model = result.Value;
            return new ModelCard
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
