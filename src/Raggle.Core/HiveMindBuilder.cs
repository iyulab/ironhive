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
    public IHiveServiceContainer Services { get; }

    public HiveMindBuilder()
    {
        Services = new HiveServiceContainer();
    }

    public IHiveMindBuilder AddKeyedService<TService>(string serviceKey, TService instance)
        where TService : class
    {
        Services.RegisterKeyedService(serviceKey, instance);
        return this;
    }

    public IHiveMindBuilder AddChatCompletionConnector(string serviceKey, IChatCompletionConnector connector)
        => AddKeyedService(serviceKey, connector);

    public IHiveMindBuilder AddEmbeddingConnector(string serviceKey, IEmbeddingConnector connector)
        => AddKeyedService(serviceKey, connector);

    public IHiveMindBuilder AddFileStorage(string serviceKey, IFileStorage storage)
        => AddKeyedService(serviceKey, storage);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHiveServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatCompletionService, ChatCompletionService>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IModelParser, ServiceModelParser>();
        return services;
    }
}
