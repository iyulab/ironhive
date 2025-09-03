using System.Net;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Collections;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class ModelCatalogService : IModelCatalogService
{
    private readonly IProviderCollection _providers;

    public ModelCatalogService(IProviderCollection providers)
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
            if (_providers.TryGet<IModelCatalog>(provider, out var service))
            {
                var models = await service.ListModelsAsync(cancellationToken);
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
    public async Task<IEnumerable<ModelSpecItem<T>>> ListModelsAsync<T>(
        string? provider, 
        CancellationToken cancellationToken = default)
        where T : IModelSpec
    {
        throw new NotImplementedException();
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
