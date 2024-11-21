using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions.Extensions;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddChatCompletionService<T>(
        this IServiceCollection services,
        object key, 
        T service)
        where T : class, IChatCompletionService
    {
        return services.AddKeyedSingleton<IChatCompletionService>(key, service);
    }

    public static IServiceCollection AddEmbeddingService<T>(
        this IServiceCollection services, 
        object key, 
        T service)
        where T : class, IEmbeddingService
    {
        return services.AddKeyedSingleton<IEmbeddingService>(key, service);
    }

    public static IServiceCollection AddDocumentStorage<T>(
        this IServiceCollection services, 
        object key, 
        T service)
        where T : class, IDocumentStorage
    {
        return services.AddKeyedSingleton<IDocumentStorage>(key, service);
    }

    public static IServiceCollection AddVectorStorage<T>(
        this IServiceCollection services, 
        object key, 
        T service)
        where T : class, IVectorStorage
    {
        return services.AddKeyedSingleton<IVectorStorage>(key, service);
    }
}
