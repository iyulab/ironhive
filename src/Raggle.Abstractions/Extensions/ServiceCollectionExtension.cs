using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;

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

    /// <summary>
    /// Copies Raggle services from the source service collection to the target service collection.
    /// </summary>
    /// <param name="source">IServiceCollection used as the source of the services to copy.</param>
    /// <returns></returns>
    public static IServiceCollection CopyRaggleServicesFrom(
        this IServiceCollection target,
        IServiceCollection source)
    {
        foreach (var service in source)
        {
            if (IsRaggleService(service) && !target.Contains(service))
            {
                target.Add(service);
            }
        }
        return target;
    }

    #region Private Methods

    private static bool IsRaggleService(ServiceDescriptor service)
    {
        if (service.IsKeyedService)
        {
            return service.ServiceType == typeof(IDocumentStorage)
            || service.ServiceType == typeof(IVectorStorage)
            || service.ServiceType == typeof(IChatCompletionService)
            || service.ServiceType == typeof(IEmbeddingService)
            || service.ServiceType == typeof(IPipelineHandler);
        }
        else
        {
            return service.ServiceType == typeof(IDocumentDecoder);
        }
    }

    #endregion

    #region 보류

    //public static IServiceCollection AddToolkit(
    //    this IServiceCollection services,
    //    object key,
    //    ToolKit toolkit)
    //{
    //    return services.AddKeyedSingleton(key, toolkit);
    //}

    #endregion
}
