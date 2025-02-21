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
    public IHiveServiceRegistry Registry { get; }

    public HiveServiceBuilder(IServiceCollection services)
    {
        Services = services;
        Registry = new HiveServiceRegistry();
        Services.AddSingleton(Registry);
    }

    public IHiveServiceBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddSingleton<TService, TImplementation>();
        return this;
    }

    public IHiveServiceBuilder AddKeyedService<TService>(string serviceKey, TService instance)
        where TService : class
    {
        Registry.RegisterKeyedService(serviceKey, instance);
        return this;
    }

    public IHiveServiceBuilder AddChatCompletionConnector(string serviceKey, IChatCompletionConnector connector)
        => AddKeyedService(serviceKey, connector);

    public IHiveServiceBuilder AddEmbeddingConnector(string serviceKey, IEmbeddingConnector connector)
        => AddKeyedService(serviceKey, connector);

    public IHiveServiceBuilder AddFileStorage(string serviceKey, IFileStorage storage)
        => AddKeyedService(serviceKey, storage);
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

        builder.AddService<ITextParser<(string, string)>, ModelIdentifierParser>();

        return builder;
    }
}
