using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Decoders;

namespace Raggle.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentDecoder<T>(
        this IServiceCollection services)
        where T : class, IDocumentDecoder
    {
        return services.AddSingleton<IDocumentDecoder, T>();
    }

    public static IServiceCollection AddPipelineHandler<T>(
        this IServiceCollection services,
        string serviceKey)
        where T : class, IPipelineHandler
    {
        ServiceKeyRegistry.Add<T>(serviceKey);
        return services.AddKeyedSingleton<IPipelineHandler, T>(serviceKey);
    }

    public static IServiceCollection AddToolService<T>(
        this IServiceCollection services,
        string serviceKey)
        where T : class
    {
        ServiceKeyRegistry.Add<T>(serviceKey);
        return services.AddKeyedSingleton<T>(serviceKey);
    }
}
