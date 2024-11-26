using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions.Extensions;

public static partial class IServiceCollectionExtensions
{
    // Only One Singleton
    public static IServiceCollection SetDocumentStorage<T>(
        this IServiceCollection services,
        T implementationInstance)
        where T : class, IDocumentStorage
    {
        services.RemoveAll<IDocumentStorage>();
        return services.AddSingleton<IDocumentStorage>(implementationInstance);
    }

    // Only One Singleton
    public static IServiceCollection SetVectorStorage<T>(
        this IServiceCollection services,
        T implementationInstance)
        where T : class, IVectorStorage
    {
        services.RemoveAll<IVectorStorage>();
        return services.AddSingleton<IVectorStorage>(implementationInstance);
    }

    // Keyed Singletons
    public static IServiceCollection AddChatCompletionService<T>(
        this IServiceCollection services,
        string serviceKey,
        T implementationInstance)
        where T : class, IChatCompletionService
    {
        return services.AddKeyedSingleton<IChatCompletionService>(serviceKey, implementationInstance);
    }

    // Keyed Singletons
    public static IServiceCollection AddEmbeddingService<T>(
        this IServiceCollection services, 
        string serviceKey, 
        T implementationInstance)
        where T : class, IEmbeddingService
    {
        return services.AddKeyedSingleton<IEmbeddingService>(serviceKey, implementationInstance);
    }

    // Keyed Singletons
    public static IServiceCollection AddPipelineHandler<T>(
        this IServiceCollection services,
        string serviceKey)        
        where T : class, IPipelineHandler
    {
        return services.AddKeyedSingleton<IPipelineHandler, T>(serviceKey);
    }

    // Multiple Singletons
    public static IServiceCollection AddDocumentDecoder<T>(
        this IServiceCollection services)
        where T : class, IDocumentDecoder
    {
        return services.AddSingleton<IDocumentDecoder, T>();
    }

    public static IServiceCollection CopyServicesFrom<T>(
        this IServiceCollection target,
        IServiceCollection source)
    {
        foreach (var service in source)
        {
            if (service.ServiceType == typeof(T) && !target.Contains(service))
            {
                target.Add(service);
            }
        }
        return target;
    }

    public static IServiceCollection CopyRaggleServicesFrom(
        this IServiceCollection target,
        IServiceCollection source)
    {
        foreach (var service in source)
        {
            if (target.Contains(service))
            {
                continue;
            }

            bool isRaggleService;
            if (service.IsKeyedService)
            {
                // Keyed Service
                isRaggleService = service.ServiceType == typeof(IChatCompletionService)
                    || service.ServiceType == typeof(IEmbeddingService)
                    || service.ServiceType == typeof(IPipelineHandler);
            }
            else
            {
                // Singleton Service
                isRaggleService = service.ServiceType == typeof(IDocumentStorage)
                    || service.ServiceType == typeof(IVectorStorage)
                    || service.ServiceType == typeof(IDocumentDecoder);
            }

            if (isRaggleService)
            {
                target.Add(service);
            }
        }
        return target;
    }
}
