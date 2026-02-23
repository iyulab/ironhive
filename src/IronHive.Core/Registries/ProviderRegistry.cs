using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Registries;

namespace IronHive.Core.Registries;

/// <inheritdoc />
public class ProviderRegistry : KeyedServiceRegistry<string, IProviderItem>, IProviderRegistry
{ 
    public ProviderRegistry() : base() { }

    public ProviderRegistry(StringComparer comparer) : base(comparer) { }

    /// <inheritdoc />
    public void SetModelCatalog(string providerName, IModelCatalog catalog)
        => Set<IModelCatalog>(providerName, catalog);

    /// <inheritdoc />
    public void SetMessageGenerator(string providerName, IMessageGenerator generator)
        => Set<IMessageGenerator>(providerName, generator);

    /// <inheritdoc />
    public void SetEmbeddingGenerator(string providerName, IEmbeddingGenerator generator)
        => Set<IEmbeddingGenerator>(providerName, generator);

    /// <inheritdoc />
    public void SetImageGenerator(string providerName, IImageGenerator generator)
        => Set<IImageGenerator>(providerName, generator);
}
