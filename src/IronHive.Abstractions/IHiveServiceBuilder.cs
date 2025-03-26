using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;

namespace Microsoft.Extensions.DependencyInjection;

public interface IHiveServiceBuilder
{
    IHiveServiceBuilder AddChatCompletionConnector(string serviceKey, IChatCompletionConnector connector);

    IHiveServiceBuilder AddEmbeddingConnector(string serviceKey, IEmbeddingConnector connector);

    IHiveServiceBuilder AddToolHandler<TService>(string serviceKey, ServiceLifetime lifetime = ServiceLifetime.Singleton, Func<IServiceProvider, object?, TService>? implementationFactory = null) where TService : class, IToolHandler;

    IHiveServiceBuilder WithVectorStorage(IVectorStorage storage);

    IHiveServiceBuilder WithQueueStorage(IQueueStorage storage);

    IHiveServiceBuilder AddPipelineHandler<TService>(string serviceKey, ServiceLifetime lifetime = ServiceLifetime.Singleton, Func<IServiceProvider, object?, TService>? implementationFactory = null) where TService : class, IPipelineHandler;
}
