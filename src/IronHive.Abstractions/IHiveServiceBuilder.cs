using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions;

/// <summary>
/// Builder interface for registering Hive services and creating a HiveMind instance.
/// </summary>
public interface IHiveServiceBuilder
{
    #region AI Services

    /// <summary>
    /// Registers a chat completion connector in the Hive service store.
    /// </summary>
    /// <param name="serviceKey">
    /// A unique key used to identify the service provider for chat completion.
    /// </param>
    /// <param name="connector">The chat completion connector implementation.</param>
    IHiveServiceBuilder AddChatCompletionConnector(
        string serviceKey,
        IChatCompletionConnector connector);

    /// <summary>
    /// Registers an embedding connector in the Hive service store.
    /// </summary>
    /// <param name="serviceKey">
    /// A unique key used to identify the service provider for embedding.
    /// </param>
    /// <param name="connector">The embedding connector implementation.</param>
    IHiveServiceBuilder AddEmbeddingConnector(
        string serviceKey,
        IEmbeddingConnector connector);

    #endregion

    #region Memory Services

    /// <summary>
    /// Registers a queue storage as a singleton in the service collection.
    /// Only one queue storage can be registered; a new registration replaces the previous one.
    /// </summary>
    IHiveServiceBuilder WithQueueStorage(IQueueStorage storage);

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
    /// <param name="serviceKey">
    /// A unique key used to identify the file storage provider in the file service.
    /// </param>
    /// <param name="implementationFactory">
    /// A factory method to create the file storage.
    /// The first parameter is the service provider and the second is a user-defined configuration object.
    /// If null, the default implementation is used.
    /// </param>
    IHiveServiceBuilder AddFileStorage<TImplementation>(
        string serviceKey,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TImplementation : class, IFileStorage;

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
