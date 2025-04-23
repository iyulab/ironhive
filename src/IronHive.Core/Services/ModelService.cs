using IronHive.Abstractions;
using IronHive.Abstractions.Models;
using System.Net;

namespace IronHive.Core.Services;

public class ModelService : IModelService
{
    private readonly IHiveServiceStore _store;

    public ModelService(IHiveServiceStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelDescriptor>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ModelDescriptor>();
        var connectors = _store.GetServices<IModelConnector>();

        foreach (var (key, conn) in connectors)
        {
            try
            {
                var providerModels = await conn.ListModelsAsync(cancellationToken);
                models.AddRange(providerModels.Select(m =>
                {
                    m.Provider = key;
                    return m;
                }));
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
    public async Task<ModelDescriptor?> GetModelAsync(
        string provider, 
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var conn = _store.GetService<IModelConnector>(provider);
        var providerModel = await conn.GetModelAsync(modelId, cancellationToken);
        if (providerModel is not null)
        {
            providerModel.Provider = provider;
        }
        return providerModel;
    }
}
