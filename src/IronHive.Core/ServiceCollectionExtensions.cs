using IronHive.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        var builder = new HiveServiceBuilder(services);
        builder.AddDefaultServices();
        return builder;
    }
}
