using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;
using Raggle.Abstractions.Memory;
using Raggle.Core.ChatCompletion;
using Raggle.Core.Embedding;

namespace Raggle.Core;

public class HiveMindBuilder : IHiveMindBuilder
{
    public IServiceCollection Services { get; }
    public IHiveServiceContainer Container { get; }

    public HiveMindBuilder()
    {
        Services = new ServiceCollection();
        Container = new HiveServiceContainer();
    }

    public HiveMindBuilder(IServiceCollection services)
    {
        Services = services;
        Container = new HiveServiceContainer();
        Services.AddSingleton(Container);
    }

    public IHiveMindBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddSingleton<TService, TImplementation>();
        return this;
    }

    public IHiveMindBuilder AddKeyedService<TService, TImplementation>(string serviceKey, TImplementation instance)
        where TService : class
        where TImplementation : class, TService
    {
        Container.RegisterKeyedService<TService, TImplementation>(serviceKey, instance);
        return this;
    }

    public IHiveMindBuilder AddChatCompletionConnector<T>(string serviceKey, T connector)
        where T : class, IChatCompletionConnector
        => AddKeyedService<IChatCompletionConnector, T>(serviceKey, connector);

    public IHiveMindBuilder AddEmbeddingConnector<T>(string serviceKey, T connector)
        where T : class, IEmbeddingConnector
        => AddKeyedService<IEmbeddingConnector, T>(serviceKey, connector);

    public IHiveMindBuilder AddFileStorage<T>(string serviceKey, T storage)
        where T : class, IFileStorage
        => AddKeyedService<IFileStorage, T>(serviceKey, storage);

    public IHiveMind Build()
    {
        Services.AddSingleton<IModelParser, ServiceModelParser>();
        Services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        Services.AddSingleton<IEmbeddingService, EmbeddingService>();
        var provider = Services.BuildServiceProvider();
        return new HiveMind(provider);
    }
}

public static class ServiceCollectionExtensions
{
    public static IHiveMindBuilder AddHiveServices(this IServiceCollection services)
    {
        var builder = new HiveMindBuilder(services);

        builder.AddService<IChatCompletionService, ChatCompletionService>();
        builder.AddService<IEmbeddingService, EmbeddingService>();

        // 메모리 서비스(벡터 스토리지) ~

        // AI 서비스 ~

        // 파일 스토리지 ~

        builder.AddService<IModelParser, ServiceModelParser>();

        return builder;
    }
}
