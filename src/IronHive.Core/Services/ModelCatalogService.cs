using System.Net;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Registries;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ModelCatalogService : IModelCatalogService
{
    private readonly IProviderRegistry _providers;

    public ModelCatalogService(IProviderRegistry providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSpecList>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.WhenAll(_providers.Entries<IModelCatalog>().Select(async (kvp) =>
        {
            try
            {
                var models = await kvp.Value.ListModelsAsync(cancellationToken);
                return new ModelSpecList
                {
                    Provider = kvp.Key,
                    Models = models
                };
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // 401 Unauthorized는 무시
                return new ModelSpecList
                {
                    Provider = kvp.Key,
                    Models = Enumerable.Empty<IModelSpec>()
                };
            }
        }));
    }

    /// <inheritdoc />
    public async Task<ModelSpecList?> ListModelsAsync(
        string provider,
        CancellationToken cancellationToken = default)
    {
        if (_providers.TryGet<IModelCatalog>(provider, out var catalog))
        {
            var models = await catalog.ListModelsAsync(cancellationToken);
            return new ModelSpecList
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
    public async Task<IModelSpec?> FindModelAsync(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGet<IModelCatalog>(provider, out var catalog))
            return null;
        
        return await catalog.FindModelAsync(modelId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSpecList<T>>> ListModelsAsync<T>(
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec
    {
        var list = await ListModelsAsync(cancellationToken);
        return list.Select(msl => new ModelSpecList<T>
        {
            Provider = msl.Provider,
            Models = msl.Models.OfType<T>()
        });
    }

    /// <inheritdoc />
    public async Task<ModelSpecList<T>?> ListModelsAsync<T>(
        string provider, 
        CancellationToken cancellationToken = default) 
        where T : class, IModelSpec
    {
        var models = await ListModelsAsync(provider, cancellationToken);
        return models == null ? null : new ModelSpecList<T>
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
        where T : class, IModelSpec
    {
        var model = await FindModelAsync(provider, modelId, cancellationToken);
        return model is T typedModel ? typedModel : null;
    }
}
