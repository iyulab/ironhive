using System.Net;
using IronHive.Abstractions;
using IronHive.Abstractions.Catalog;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ModelCatalogService : IModelCatalogService
{
    private readonly IKeyedCollection<IModelCatalogProvider> _providers;

    public ModelCatalogService(IKeyedCollectionGroup<IKeyedProvider> providers)
    {
        _providers = providers.Of<IModelCatalogProvider>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (_providers.TryGet(provider, out var service))
            {
                return await service.ListModelsAsync(cancellationToken);
            }
            else
            {
                return Enumerable.Empty<ModelSummary>();
            }
        }
        else
        {
            var tasks = _providers.Values.Select(async p =>
            {
                try
                {
                    return await p.ListModelsAsync(cancellationToken);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // 401 Unauthorized는 무시
                    return Enumerable.Empty<ModelSummary>();
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r);
        }
    }

    /// <inheritdoc />
    public async Task<ModelDetails?> FindModelAsync(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        if (_providers.TryGet(provider, out var service))
        {
            return await service.FindModelAsync(modelId, cancellationToken);
        }
        else
        {
            return null;
        }
    }
}
