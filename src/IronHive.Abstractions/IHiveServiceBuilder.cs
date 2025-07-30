using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Message;

namespace IronHive.Abstractions;

/// <summary>
/// Builder interface for registering Hive services and creating a HiveMind instance.
/// </summary>
public interface IHiveServiceBuilder
{
    /// <summary>
    /// Gets the service collection used to register services in the Hive service store.
    /// </summary>
    IServiceCollection Services { get; }

    #region AI Services

    /// <summary>
    /// Registers a model provider as a singleton in the service collection.
    /// </summary>
    IHiveServiceBuilder AddModelCatalogProvider(IModelCatalogProvider provider);

    /// <summary>
    /// Registers a message generation provider as a singleton in the service collection.
    /// </summary>
    IHiveServiceBuilder AddMessageGenerator(IMessageGenerator provider);

    /// <summary>
    /// Registers an embedding provider as a singleton in the service collection.
    /// </summary>
    IHiveServiceBuilder AddEmbeddingGenerator(IEmbeddingGenerator provider);

    /// <summary>
    /// Registers a tool plugin as a singleton in the service collection.
    /// </summary>
    IHiveServiceBuilder AddToolPlugin(IToolPlugin plugin);

    #endregion

    #region Memory Services

    /// <summary>
    /// Registers a queue storage as a singleton in the service collection.
    /// Only one queue storage can be registered; a new registration replaces the previous one.
    /// </summary>
    IHiveServiceBuilder WithQueueStorage(IQueueStorage<PipelineRequest> storage);

    /// <summary>
    /// Registers a vector storage as a singleton in the service collection.
    /// Only one vector storage can be registered; a new registration replaces the previous one.
    /// </summary>
    IHiveServiceBuilder WithVectorStorage(IVectorStorage storage);

    /// <summary>
    /// Registers one or more pipeline event observers to the service collection.
    /// </summary>
    /// <param name="lifetime">
    /// The service's lifetime. Ensure compatibility with other registered services.
    /// </param>
    /// <param name="implementationFactory">
    /// A factory method to create the pipeline observer.
    /// The first parameter is the service provider, If null, the default implementation is used.
    /// </param>
    IHiveServiceBuilder AddPipelineObserver<TImplementation>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, TImplementation>? implementationFactory = null)
        where TImplementation : class, IPipelineObserver;

    /// <summary>
    /// Adds a pipeline handler to the service collection.
    /// </summary>
    /// <param name="serviceKey">
    /// A unique key used as the pipeline step name in the memory service.
    /// </param>
    /// <param name="lifetime">
    /// The service's lifetime. Ensure compatibility with other registered services.
    /// </param>
    /// <param name="implementationFactory">
    /// A factory method to create the pipeline handler.
    /// The first parameter is the service provider and the second is the service key.
    /// If null, the default implementation is used.
    /// </param>
    IHiveServiceBuilder AddPipelineHandler<TImplementation>(
        string serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IPipelineHandler;

    #endregion

    #region File Services

    /// <summary>
    /// Registers a file storage factory as a singleton in the service collection.
    /// </summary>
    IHiveServiceBuilder AddFileStorage(IFileStorage storage);

    /// <summary>
    /// Registers one or more file decoders as a singleton in the service collection.
    /// Note: The order of decoders matters; the first decoder that supports the file type will be used.
    /// </summary>
    IHiveServiceBuilder AddFileDecoder(IFileDecoder decoder);

    #endregion

    /// <summary>
    /// Creates a standalone HiveMind instance.
    /// Use this method when not employing the service collection.
    /// </summary>
    IHiveMind BuildHiveMind();
}
