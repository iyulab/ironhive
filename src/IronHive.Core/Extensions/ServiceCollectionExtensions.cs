using IronHive.Abstractions;
using IronHive.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        var builder = new HiveServiceBuilder(services);
        return builder;
    }

    public static IHiveServiceBuilder AddHiveServiceDefault(this IServiceCollection services)
    {
        var builder = services.AddHiveServiceCore();
        builder.AddDefaultFileDecoders();
        builder.AddDefaultPipelineHandlers();
        return builder;
    }
}
