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
    public async Task<IEnumerable<ModelSpecItem>> ListModelsAsync(
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (_providers.TryGet<IModelCatalog>(provider, out var catalog))
            {
                var models = await catalog.ListModelsAsync(cancellationToken);
                return models.Select(m => new ModelSpecItem
                {
                    Provider = provider,
                    Model = m
                });
            }
            else
            {
                return Enumerable.Empty<ModelSpecItem>();
            }
        }
        else
        {
            var tasks = _providers.Entries<IModelCatalog>().Select(async (kvp) =>
            {
                try
                {
                    var models = await kvp.Value.ListModelsAsync(cancellationToken);
                    return models.Select(m => new ModelSpecItem
                    {
                        Provider = kvp.Key,
                        Model = m
                    });
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // 401 Unauthorized는 무시
                    return Enumerable.Empty<ModelSpecItem>();
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSpecItem<T>>> ListModelsAsync<T>(
        string? provider, 
        CancellationToken cancellationToken = default)
        where T : class, IModelSpec
    {
        var models = await ListModelsAsync(provider, cancellationToken);
        return models.Where(m => m.Model is T)
                     .Select(m => new ModelSpecItem<T>
                     {
                         Provider = m.Provider,
                         Model = (T)m.Model
                     });
    }

    /// <inheritdoc />
    public async Task<ModelSpecItem?> FindModelAsync(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        if (_providers.TryGet<IModelCatalog>(provider, out var service))
        {
            var model = await service.FindModelAsync(modelId, cancellationToken);
            return model is not null 
                ? new ModelSpecItem
                {
                    Provider = provider,
                    Model = model
                }
                : null;
        }
        else
        {
            return null;
        }
    }

    
}
