using System.Net;
using IronHive.Abstractions.Catalog;

namespace IronHive.Core.Services;

public class ModelCatalogService : IModelCatalogService
{
    private readonly Dictionary<string, IModelCatalogProvider> _providers;

    public ModelCatalogService(IEnumerable<IModelCatalogProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ModelSummary>();

        foreach (var provider in _providers.Values)
        {
            try
            {
                var smmaries = await provider.ListModelsAsync(cancellationToken);
                models.AddRange(smmaries);
            }
            catch(HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Ignore 401 errors for now.
                continue;
            }
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<ModelDetails?> FindModelAsync(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        if (_providers.TryGetValue(provider, out var service))
        {
            return await service.FindModelAsync(modelId, cancellationToken);
        }
        else
        {
            return null;
        }
    }
}
