using IronHive.Abstractions.Tools;
using IronHive.Plugins.MCP.Configurations;

namespace IronHive.Plugins.MCP;

public class McpServerManager : IMcpServerManager
{
    private readonly IDictionary<string, object> _servers = new Dictionary<string, object>();
    private readonly IToolPluginManager _tools;

    public McpServerManager(IToolPluginManager tools)
    {
        _tools = tools;
        var plugins = tools.Plugins.Values.OfType<McpToolPlugin>();
        _servers = new Dictionary<string, object>(plugins.Count());
        foreach (var plugin in plugins)
        {
            _servers[plugin.PluginName] = new();
        }
    }

    public IReadOnlyDictionary<string, object> Servers => (IReadOnlyDictionary<string, object>)_servers;

    public void Add(string serverName, IMcpServerConfig config)
    {
        _tools.TryAddMcpToolPlugin(serverName, config);
        _servers[serverName] = new object();
    }

    public void Remove(string serverName)
    {
        _tools.Plugins.Remove(serverName);
        _servers.Remove(serverName);
    }

    public async Task ConnectAsync(string serverName, IMcpServerConfig config, CancellationToken cancellationToken)
    {
        var plugin = _tools.Plugins[serverName] as McpToolPlugin;
        if (plugin != null)
        {
            await plugin.ConnectAsync();
        }
    }

    public async Task DisconnectAsync(string serverName, CancellationToken cancellationToken)
    {
        var plugin = _tools.Plugins[serverName] as McpToolPlugin;
        if (plugin != null)
        {
            await plugin.DisconnectAsync();
        }
    }
}
