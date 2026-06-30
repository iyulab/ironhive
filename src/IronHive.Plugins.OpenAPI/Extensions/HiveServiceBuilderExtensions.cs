using IronHive.Plugins.OpenAPI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스에 OpenAPI 클라이언트 매니저를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenApiClient(
        this IHiveServiceBuilder builder)
    {
        builder.AddToolInitializer((tools, sp) =>
        {
            _ = new OpenApiClientManager(tools);
        });
        return builder;
    }

    /// <summary>
    /// Hive 서비스에 여러 OpenAPI 클라이언트와 매니저를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenApiClient(
        this IHiveServiceBuilder builder,
        IEnumerable<OpenApiClient> clients)
    {
        builder.AddToolInitializer((tools, sp) =>
        {
            var manager = new OpenApiClientManager(tools);
            foreach (var c in clients)
                manager.AddOrUpdate(c);
        });
        return builder;
    }
}
