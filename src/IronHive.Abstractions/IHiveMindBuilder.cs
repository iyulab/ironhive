using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions;

public interface IHiveMindBuilder
{
    IHiveMindBuilder AddChatCompletionConnector(string serviceKey, IChatCompletionConnector connector);
    
    IHiveMindBuilder AddEmbeddingConnector(string serviceKey, IEmbeddingConnector connector);
    
    IHiveMindBuilder AddToolHandler<TService>(string serviceKey, ServiceLifetime lifetime = ServiceLifetime.Singleton, Func<IServiceProvider, object?, TService>? implementationFactory = null) where TService : class, IToolHandler;
    
    IHiveMindBuilder WithVectorStorage(IVectorStorage storage);
    
    IHiveMindBuilder WithQueueStorage(IQueueStorage storage);
    
    IHiveMindBuilder AddPipelineHandler<TService>(string serviceKey, ServiceLifetime lifetime = ServiceLifetime.Singleton, Func<IServiceProvider, object?, TService>? implementationFactory = null) where TService : class, IPipelineHandler;

    IHiveMind Build();
}
