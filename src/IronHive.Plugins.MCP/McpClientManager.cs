using IronHive.Abstractions.Messages;
using IronHive.Plugins.MCP.Configurations;
using System.Collections.Concurrent;

namespace IronHive.Plugins.MCP;

/// <summary>
/// Mcp 서버와의 연결을 관리하는 객체 입니다.
/// </summary>
public class McpClientManager
{
    private readonly ConcurrentDictionary<string, McpSession> _sessions = new();
    private readonly IMessageService _service;

    public McpClientManager(IMessageService service)
    {
        _service = service;
    }

    /// <summary>
    /// 모든 Mcp 서버에 대한 McpSession 인스턴스를 반환합니다.
    /// </summary>
    public IReadOnlyCollection<McpSession> GetAll()
    {
        return _sessions.Values.ToArray();
    }

    /// <summary>
    /// 해당 서버 이름에 대한 McpSession 인스턴스를 반환합니다.
    /// </summary>
    /// <param name="serverName">서버의 이름입니다.</param>
    /// <returns>지정된 서버 이름의 McpSession 인스턴스를 반환합니다. 존재하지 않는 경우 null을 반환합니다.</returns>
    public McpSession? Get(string serverName)
    {
        if (_sessions.TryGetValue(serverName, out var client))
            return client;
        return null;
    }

    /// <summary>
    /// Mcp 서버에 연결할 클라이언트를 추가합니다. 추가된 클라이언트는 비동기적으로 연결을 시도합니다.
    /// </summary>
    /// <param name="config">연결 설정입니다.</param>
    public void AddOrUpdate(IMcpClientConfig config)
    {
        _sessions.AddOrUpdate(config.ServerName, 
            (name) =>
            {
                // 새로운 세션을 생성하고 연결을 시도합니다.
                var session = new McpSession(config);
                session.Connected += OnClientConnected;
                session.Disconnected += OnClientDisconnected;
                session.Errored += OnClientErrored;
                _ = Task.Run(() => session.ConnectAsync());
                return session;
            },
            (name, session) =>
            {
                // 기존 세션이 있다면 세션 설정을 업데이트 하고 연결을 시도합니다.
                _ = Task.Run(() => session.ReconnectAsync(config));
                return session;
            }
        );
    }

    /// <summary>
    /// 지정된 서버 이름의 클라이언트를 제거합니다.
    /// </summary>
    /// <param name="serverName">제거할 서버의 이름입니다.</param>
    public void Remove(string serverName)
    {
        if (_sessions.TryRemove(serverName, out var client))
        {
            _ = Task.Run(client.DisposeAsync);
        }
    }

    /// <summary>
    /// Mcp 서버에 연결된 클라이언트가 연결되었을 때 작동하는 기본 이벤트 핸들러입니다.
    /// </summary>
    private void OnClientConnected(object? sender, McpConnectedEventArgs e)
    {
        if (sender is not McpSession client)
            return;

        client.ListToolsAsync().ContinueWith(t =>
        {
            if (t.IsFaulted || t.Result is null)
                return;

            foreach (var tool in t.Result)
            {
                _service.Tools.Set(tool);
            }
        });
    }

    /// <summary>
    /// Mcp 서버와의 연결이 해제되었을 때 작동하는 기본 이벤트 핸들러입니다.
    /// </summary>
    private void OnClientDisconnected(object? sender, McpDisconnectedEventArgs e)
    {
        if (sender is not McpSession client)
            return;

        _service.Tools.RemoveAll(t =>
        {
            if (t is not McpTool tool)
                return false;

            return tool.ServerName.Equals(client.ServerName);
        });
    }

    /// <summary>
    /// Mcp 서버와의 연결중 오류가 발생했을 때 작동하는 기본 이벤트 핸들러입니다.
    /// </summary>
    private void OnClientErrored(object? sender, McpErroredEventArgs e)
    {
        if (sender is not McpSession client)
            return;

        _service.Tools.RemoveAll(t =>
        {
            if (t is not McpTool tool)
                return false;

            return tool.ServerName.Equals(client.ServerName);
        });
    }
}
