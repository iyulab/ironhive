using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;

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

    #endregion
}
