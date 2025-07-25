using IronHive.Plugins.MCP;

namespace IronHive.Abstractions.Tools;

public static class ToolPluginManagerExtensions
{
    /// <summary>
    /// MCP 서버를 기반으로 플러그인을 생성하고 ToolPluginManager에 등록합니다.
    /// </summary>
    /// <param name="name">등록할 MCP 서버 이름</param>
    /// <param name="server">MCP 서버 인스턴스</param>
    public static bool TryAddMcpToolPlugin(
        this IToolPluginManager manager, 
        string name, 
        IMcpServerConfig server)
    {
        return manager.Plugins.TryAdd(name, new McpToolPlugin(server)
        {
            PluginName = name
        });
    }
}
