using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Hive 서비스 빌더에 MCP (ModelContext Protocol) 서버를 등록합니다.
    /// </summary>
    /// <param name="pluginName">등록할 MCP 플러그인의 이름입니다.</param>
    /// <param name="config">등록할 MCP 서버의 설정입니다.</param>    
    public static IHiveServiceBuilder AddMcpToolPlugin(
        this IHiveServiceBuilder builder, 
        string pluginName, 
        IMcpServerConfig config)
    {
        builder.AddToolPlugin(new McpToolPlugin(config)
        {
            PluginName = pluginName
        });
        return builder;
    }
}
