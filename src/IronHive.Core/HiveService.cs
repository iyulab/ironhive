using IronHive.Abstractions;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Message;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

/// <inheritdoc />
public class HiveService : IHiveService
{
    public HiveService(IServiceProvider services)
    {
        Services = services;
        File = services.GetRequiredService<IFileService>();
        Memory = services.GetRequiredService<IMemoryService>();
    }

    /// <inheritdoc />
    public IServiceProvider Services { get; }

    /// <inheritdoc />
    public IFileService File { get; }

    /// <inheritdoc />
    public IMemoryService Memory { get; }

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
        var service = Services.GetRequiredService<IMessageService>();
        service.Generators.Set(generator);
        return this;
    }

    /// <inheritDoc />
    public IHiveService SetEmbeddingGenerator(IEmbeddingGenerator generator)
    {
        var service = Services.GetRequiredService<IEmbeddingService>();
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
        var service = Services.GetRequiredService<IMessageService>();
        service.Generators.Remove(name);
        return this;
    }

    /// <inheritDoc />
    public IHiveService RemoveEmbeddingGenerator(string name)
    {
        var service = Services.GetRequiredService<IEmbeddingService>();
        service.Generators.Remove(name);
        return this;
    }
}
