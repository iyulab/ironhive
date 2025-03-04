using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;
using Raggle.Abstractions.Memory;
using Raggle.Core.ChatCompletion;
using Raggle.Core.Embedding;

namespace Raggle.Core;

public class HiveServiceBuilder : IHiveServiceBuilder
{
    public IServiceCollection Services { get; }
    public IHiveServiceContainer Registry { get; }

    public HiveServiceBuilder(IServiceCollection services)
    {
        Services = services;
        Registry = new HiveServiceContainer();
        Services.AddSingleton(Registry);
    }

    public IHiveServiceBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddSingleton<TService, TImplementation>();
        return this;
    }

    public IHiveServiceBuilder AddKeyedService<TService, TImplementation>(string serviceKey, TImplementation instance)
        where TService : class
        where TImplementation : class, TService
    {
        Registry.RegisterKeyedService<TService, TImplementation>(serviceKey, instance);
        return this;
    }

    public IHiveServiceBuilder AddChatCompletionConnector<T>(string serviceKey, T connector)
        where T : class, IChatCompletionConnector
        => AddKeyedService<IChatCompletionConnector, T>(serviceKey, connector);

    public IHiveServiceBuilder AddEmbeddingConnector<T>(string serviceKey, T connector)
        where T : class, IEmbeddingConnector
        => AddKeyedService<IEmbeddingConnector, T>(serviceKey, connector);

    public IHiveServiceBuilder AddFileStorage<T>(string serviceKey, T storage)
        where T : class, IFileStorage
        => AddKeyedService<IFileStorage, T>(serviceKey, storage);
}

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServices(this IServiceCollection services)
    {
        var builder = new HiveServiceBuilder(services);

        builder.AddService<IChatCompletionService, ChatCompletionService>();
        builder.AddService<IEmbeddingService, EmbeddingService>();

        // 메모리 서비스(벡터 스토리지) ~

        // AI 서비스 ~

        // 파일 스토리지 ~

        builder.AddService<ITextParser<(string, string)>, ServiceModelParser>();

        return builder;
    }
}
