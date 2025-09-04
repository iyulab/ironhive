using IronHive.Abstractions.Tools;
using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스에 MCP (ModelContext Protocol) 클라이언트를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddMcpClient(
        this IHiveServiceBuilder builder)
    {
        builder.Services.AddSingleton<McpClientManager>();
        return builder;
    }

    /// <summary>
    /// Hive 서비스에 MCP (ModelContext Protocol) 클라이언트를 등록합니다.
    /// 주어진 설정을 기반으로 여러 개의 MCP 연결 클라이언트들이 자동으로 추가됩니다.
    /// </summary>
    public static IHiveServiceBuilder AddMcpClient(
        this IHiveServiceBuilder builder, 
        IEnumerable<IMcpClientConfig> configs)
    {
        builder.Services.AddSingleton(sp =>
        {
            var tools = sp.GetRequiredService<IToolCollection>();
            var manager = new McpClientManager(tools);
            foreach (var config in configs)
            {
                manager.AddOrUpdate(config);
            }
            return manager;
        });
        return builder;
    }
}
