using IronHive.Abstractions;
using IronHive.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 서비스 Collection에 IronHive 서비스를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IHiveService, HiveService>();
        var builder = new HiveServiceBuilder(services);
        return builder;
    }
}
