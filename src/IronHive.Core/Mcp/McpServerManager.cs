using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

namespace IronHive.Core.Mcp;

/// <inheritdoc />
public class McpServerManager : IMcpServerManager
{
    private readonly IDictionary<string, IMcpClient> _servers = new Dictionary<string, IMcpClient>();

    /// <inheritdoc />
    public async Task<bool> IsRunningAsync(string id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _servers.ContainsKey(id);
    }

    /// <inheritdoc />
    public async Task StartAsync(string id, IMcpServer server, CancellationToken cancellationToken = default)
    {
        await StopAsync(id, cancellationToken);

        IClientTransport transport;
        if (server is McpStdioServer stdio)
        {
            transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = stdio.Command,
                Name = stdio.Name,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory,
            });
        }
        else if (server is McpSseServer sse)
        {
            transport = new SseClientTransport(new SseClientTransportOptions
            {
                Name = sse.Name,
                Endpoint = sse.Endpoint,
                AdditionalHeaders = sse.AdditionalHeaders,
                ConnectionTimeout = sse.ConnectionTimeout,
                UseStreamableHttp = sse.UseStreamableHttp
            });
        }
        else
        {
            throw new NotSupportedException($"Server type {server.GetType().Name} is not supported.");
        }

        var client = await McpClientFactory.CreateAsync(
            clientTransport: transport,
            clientOptions: null,
            cancellationToken: cancellationToken);

        if (client == null)
        {
            throw new InvalidOperationException($"Failed to create MCP client for server {server.Name}.");
        }
        _servers.Add(id, client);
    }

    /// <inheritdoc />
    public async Task StopAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_servers.TryGetValue(id, out var client))
        {
            _servers.Remove(id);
            await client.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITool>> ListToolsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_servers.TryGetValue(id, out var client))
        {
            var tools = await client.ListToolsAsync(null, cancellationToken);
            return tools.Select(tool =>
            {
                return new McpServerTool(tool);
            });
        }
        return Enumerable.Empty<ITool>();
    }
}
