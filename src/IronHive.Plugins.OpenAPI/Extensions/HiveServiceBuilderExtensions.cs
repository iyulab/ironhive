using IronHive.Abstractions.Tools;
using IronHive.Plugins.OpenAPI;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스에 OpenAPI 클라이언트 매니저를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenApiClient(
        this IHiveServiceBuilder builder)
    {
        builder.Services.AddSingleton<OpenApiClientManager>();
        return builder;
    }

    /// <summary>
    /// Hive 서비스에 여러 OpenAPI 클라이언트와 매니저를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddMcpClient(
        this IHiveServiceBuilder builder, 
        IEnumerable<OpenApiClient> clients)
    {
        builder.Services.AddSingleton(sp =>
        {
            var tools = sp.GetRequiredService<IToolCollection>();
            var manager = new OpenApiClientManager(tools);
            foreach (var c in clients)
            {
                manager.AddOrUpdate(c);
            }
            return manager;
        });
        return builder;
    }
}
