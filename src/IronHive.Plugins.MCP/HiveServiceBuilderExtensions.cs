using IronHive.Plugins.MCP;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스 빌더에 MCP (ModelContext Protocol) 서버를 등록합니다.
    /// </summary>
    /// <param name="name">등록할 MCP 서버의 이름입니다.</param>
    /// <param name="server">등록할 MCP 서버의 구현체입니다.</param>    
    public static IHiveServiceBuilder AddMcpServer(this IHiveServiceBuilder builder, string name, IMcpServer server)
    {
        var plugin = McpToolPuginFactory.Create(name, server);
        builder.AddToolPlugin(plugin);
        return builder;
    }
}
