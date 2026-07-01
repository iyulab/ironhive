using System.Net;
using IronHive.Abstractions.Models;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ModelService : IModelService
{
    private readonly IReadOnlyDictionary<string, IModelFinder> _catalogs;

    internal ModelService(IReadOnlyDictionary<string, IModelFinder> catalogs)
    {
        _catalogs = catalogs;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelCardList>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.WhenAll(_catalogs.Select(async (kvp) =>
        {
            try
            {
                var models = await kvp.Value.ListModelsAsync(cancellationToken);
                return new ModelCardList
                {
                    Provider = kvp.Key,
                    Models = models
                };
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // 401 Unauthorized는 무시
                return new ModelCardList
                {
                    Provider = kvp.Key,
                    Models = Enumerable.Empty<IModelCard>()
                };
            }
        }));
    }

    /// <inheritdoc />
    public async Task<ModelCardList?> ListModelsAsync(
        string provider,
        CancellationToken cancellationToken = default)
    {
        if (_catalogs.TryGetValue(provider, out var catalog))
        {
            var models = await catalog.ListModelsAsync(cancellationToken);
            return new ModelCardList
            {
                Provider = provider,
                Models = models
            };
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IModelCard?> FindModelAsync(
        string provider,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        if (!_catalogs.TryGetValue(provider, out var catalog))
            return null;

        return await catalog.FindModelAsync(modelId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelCardList<T>>> ListModelsAsync<T>(
        CancellationToken cancellationToken = default)
        where T : class, IModelCard
    {
        var list = await ListModelsAsync(cancellationToken);
        return list.Select(msl => new ModelCardList<T>
        {
            Provider = msl.Provider,
            Models = msl.Models.OfType<T>()
        });
    }

    /// <inheritdoc />
    public async Task<ModelCardList<T>?> ListModelsAsync<T>(
        string provider, 
        CancellationToken cancellationToken = default) 
        where T : class, IModelCard
    {
        var models = await ListModelsAsync(provider, cancellationToken);
        return models == null ? null : new ModelCardList<T>
        {
            Provider = models.Provider,
            Models = models.Models.OfType<T>()
        };
    }

    /// <inheritdoc />
    public async Task<T?> FindModelAsync<T>(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
        where T : class, IModelCard
    {
        var model = await FindModelAsync(provider, modelId, cancellationToken);
        return model is T typedModel ? typedModel : null;
    }
}
