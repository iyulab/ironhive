using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Abstractions.Json;

namespace Raggle.Abstractions.Extensions;

public static partial class IServiceCollectionExtensions
{
    #region Only One Singleton Service

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

    #endregion

    #region Keyed Singlton Services

    public static IServiceCollection AddChatCompletionService<T>(
        this IServiceCollection services,
        string serviceKey,
        T implementationInstance)
        where T : class, IChatCompletionService
    {
        return services.AddKeyedSingleton<IChatCompletionService>(serviceKey, implementationInstance);
    }

    public static IServiceCollection AddEmbeddingService<T>(
        this IServiceCollection services, 
        string serviceKey, 
        T implementationInstance)
        where T : class, IEmbeddingService
    {
        return services.AddKeyedSingleton<IEmbeddingService>(serviceKey, implementationInstance);
    }

    public static IServiceCollection AddPipelineHandler<T>(
        this IServiceCollection services,
        string serviceKey)
        where T : class, IPipelineHandler
    {
        return services.AddKeyedSingleton<IPipelineHandler, T>(serviceKey);
    }

    public static IServiceCollection AddToolKit<T>(
        this IServiceCollection services,
        string serviceKey)
        where T : class, IToolKit
    {
        return services.AddKeyedSingleton<IToolKit, T>(serviceKey);
    }

    #endregion

    // Multiple Singleton Services
    public static IServiceCollection AddDocumentDecoder<T>(
        this IServiceCollection services)
        where T : class, IDocumentDecoder
    {
        return services.AddSingleton<IDocumentDecoder, T>();
    }

    #region Utility Methods

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
                    || service.ServiceType == typeof(IPipelineHandler)
                    || service.ServiceType == typeof(string);
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

    #endregion
}
