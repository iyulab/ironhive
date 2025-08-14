using IronHive.Plugins.MCP;
using IronHive.Plugins.MCP.Configurations;

namespace IronHive.Abstractions.Tools;

public static class ToolPluginManagerExtensions
{
    /// <summary>
    /// MCP 서버 설정을 기반으로 플러그인을 생성하고 ToolPluginManager에 등록합니다.
    /// </summary>
    /// <param name="pluginName">등록할 MCP 서버 이름</param>
    /// <param name="server">MCP 서버 설정 객체</param>
    public static void AddMcpToolPlugin(
        this IToolPluginManager manager, 
        string pluginName,
        IMcpServerConfig config)
    {
        manager.Plugins.Add(new McpToolPlugin(config)
        {
            PluginName = pluginName
        });
    }
}
