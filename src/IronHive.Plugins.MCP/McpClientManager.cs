using IronHive.Plugins.MCP.Configurations;
using System.Collections.Concurrent;

namespace IronHive.Plugins.MCP;

/// <summary>
/// Mcp 서버와의 연결을 관리하는 객체 입니다.
/// </summary>
public class McpClientManager
{
    private readonly ConcurrentDictionary<string, McpClient> _clients = new();

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Add(string serverName, IMcpClientConfig config)
    {
        var client = new McpClient(config)
        {
            ServerName = serverName
        };
        _clients.TryAdd(serverName, client);
    }
}
