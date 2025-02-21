using Microsoft.Extensions.DependencyInjection.Extensions;
using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.Embedding;
using Raggle.Abstractions.Memory;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetDocumentStorage<T>(
        this IServiceCollection services,
        T implementationInstance)
        where T : class, IDocumentStorage
    {
        services.RemoveAll<IDocumentStorage>();
        return services.AddSingleton<IDocumentStorage>(implementationInstance);
    }

    public static IServiceCollection SetVectorStorage<T>(
        this IServiceCollection services,
        T implementationInstance)
        where T : class, IVectorStorage
    {
        services.RemoveAll<IVectorStorage>();
        return services.AddSingleton<IVectorStorage>(implementationInstance);
    }

    public static IServiceCollection AddChatCompletionConnector<T>(
        this IServiceCollection services,
        string serviceKey,
        T implementationInstance)
        where T : class, IChatCompletionAdapter
    {
        ServiceKeyRegistry.Add<T>(serviceKey);
        return services.AddKeyedSingleton<IChatCompletionAdapter>(serviceKey, implementationInstance);
    }

    public static IServiceCollection AddEmbeddingConnector<T>(
        this IServiceCollection services, 
        string serviceKey, 
        T implementationInstance)
        where T : class, IEmbeddingAdapter
    {
        ServiceKeyRegistry.Add<T>(serviceKey);
        return services.AddKeyedSingleton<IEmbeddingAdapter>(serviceKey, implementationInstance);
    }
}
