using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스 빌더에 MCP (ModelContext Protocol) 서버를 등록합니다.
    /// </summary>    
    public static IHiveServiceBuilder AddMcpClientWithTools(
        this IHiveServiceBuilder builder, 
        IDictionary<string, IMcpClientConfig> configs)
    {
        builder.Services.AddSingleton<McpClientManager>();
        return builder;
    }
}
