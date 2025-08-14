using System.Net;
using IronHive.Abstractions;
using IronHive.Abstractions.Catalog;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ModelCatalogService : IModelCatalogService
{
    public ModelCatalogService()
        : this(Enumerable.Empty<IModelCatalogProvider>())
    { }

    public ModelCatalogService(IEnumerable<IModelCatalogProvider> providers)
    {
        Providers = new KeyedCollection<IModelCatalogProvider>(
            providers,
            provider => provider.ProviderName);
    }

    public IKeyedCollection<IModelCatalogProvider> Providers { get; }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(
        string? provider = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (Providers.TryGet(provider, out var service))
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
            var tasks = Providers.Values.Select(async p =>
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
        if (Providers.TryGet(provider, out var service))
        {
            return await service.FindModelAsync(modelId, cancellationToken);
        }
        else
        {
            return null;
        }
    }
}
