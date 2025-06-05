using IronHive.Abstractions;
using IronHive.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IHiveMind, HiveMind>();
        var builder = new HiveServiceBuilder(services);
        return builder;
    }

    public static IHiveServiceBuilder AddDefaultHiveServices(this IServiceCollection services)
    {
        var builder = services.AddHiveServiceCore();
        builder.AddDefaultFileDecoders();
        builder.AddDefaultPipelineHandlers();
        return builder;
    }
}
