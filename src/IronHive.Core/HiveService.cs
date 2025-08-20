using IronHive.Abstractions;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Message;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

public class HiveService : IHiveService
{
    public IServiceProvider Services { get; }

    public HiveService(IServiceProvider services)
    {
        Services = services;
    }

    /// <inheritDoc />
    public IHiveService SetModelCatalogProvider(IModelCatalogProvider provider)
    {
        var service = Services.GetRequiredService<IModelCatalogService>();
        service.Providers.Set(provider);
        return this;
    }

    /// <inheritDoc />
    public IHiveService SetMessageGenerator(IMessageGenerator generator)
    {
        var service = Services.GetRequiredService<IMessageGenerationService>();
        service.Generators.Set(generator);
        return this;
    }

    /// <inheritDoc />
    public IHiveService SetEmbeddingGenerator(IEmbeddingGenerator generator)
    {
        var service = Services.GetRequiredService<IEmbeddingGenerationService>();
        service.Generators.Set(generator);
        return this;
    }

    /// <inheritDoc />
    public IHiveService RemoveModelCatalogProvider(string name)
    {
        var service = Services.GetRequiredService<IModelCatalogService>();
        service.Providers.Remove(name);
        return this;
    }

    /// <inheritDoc />
    public IHiveService RemoveMessageGenerator(string name)
    {
        var service = Services.GetRequiredService<IMessageGenerationService>();
        service.Generators.Remove(name);
        return this;
    }

    /// <inheritDoc />
    public IHiveService RemoveEmbeddingGenerator(string name)
    {
        var service = Services.GetRequiredService<IEmbeddingGenerationService>();
        service.Generators.Remove(name);
        return this;
    }
}
