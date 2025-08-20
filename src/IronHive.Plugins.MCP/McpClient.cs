using System.Data;
using ModelContextProtocol.Client;
using IronHive.Plugins.MCP.Configurations;
using IronHive.Plugins.MCP;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트를 기반으로 연결을 관리합니다.
/// MCP 서버와의 통신을 통해 도구 목록을 조회하거나 도구를 실행하는 기능을 제공합니다.
/// </summary>
public class McpClient
{
    private readonly IMcpClientConfig _config;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private IMcpClient? _client;
    private volatile bool _isDisposed = false;

    public McpClient(IMcpClientConfig config)
    {
        _config = config;

        // 서버 설정을 기반으로 MCP 클라이언트를 초기화합니다
        if (_config.AutoConnectOnCreated)
            _ = Task.Run(async () => await ConnectAsync());
    }
    
    /// <summary>
    /// 현재 연결된 서버의 이름을 나타냅니다.
    /// <summary>
    public required string ServerName { get; init; }

    /// <summary>
    /// 현재 MCP 서버와의 연결 상태를 나타냅니다.
    /// </summary>
    public McpConnectionState State { get; private set; } = McpConnectionState.Disconnected;

    /// <summary>
    /// MCP 서버에 연결이 성공했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<McpConnectedEventArgs>? Connected;

    /// <summary>
    /// MCP 서버와의 연결이 해제되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<McpDisconnectedEventArgs>? Disconnected;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        if (_client != null)
        {
            _client.DisposeAsync().GetAwaiter().GetResult();
            _client = null;
        }
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// MCP 서버에 연결합니다.
    /// </summary>
    public async Task ConnectAsync(
        McpClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (State != McpConnectionState.Disconnected)
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            State = McpConnectionState.Connecting;

            var transport = CreateTransport(_config);
            _client = await McpClientFactory.CreateAsync(
                clientTransport: transport,
                clientOptions: options,
                cancellationToken: cancellationToken);

            // 서버와의 연결을 확인합니다.
            await _client.PingAsync(cancellationToken: cancellationToken);

            State = McpConnectionState.Connected;
            Connected?.Invoke(this, new McpConnectedEventArgs(ServerName));
        }
        catch (Exception ex)
        {
            State = McpConnectionState.Disconnected;
            Disconnected?.Invoke(this, new McpDisconnectedEventArgs(ServerName, ex));
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// MCP 서버와의 연결을 해제합니다.
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null || State != McpConnectionState.Connected)
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            State = McpConnectionState.Disconnecting;
            await _client.DisposeAsync();
            _client = null;

            State = McpConnectionState.Disconnected;
            Disconnected?.Invoke(this, new McpDisconnectedEventArgs(ServerName));
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<McpTool>> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return [];

        var tools = await _client.ListToolsAsync(cancellationToken: cancellationToken);
        return tools.Select(t =>
        {
            return new McpTool(t)
            {
                ServerName = ServerName,
            };
        });
    }

    /// <summary>
    /// 전송 계층을 생성합니다.
    /// </summary>
    private static IClientTransport CreateTransport(IMcpClientConfig server)
    {
        return server switch
        {
            McpStdioClientConfig stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = stdio.Command,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory
            }),
            McpSseClientConfig sse => new SseClientTransport(new SseClientTransportOptions
            {
                Endpoint = sse.Endpoint,
                AdditionalHeaders = sse.AdditionalHeaders,
                ConnectionTimeout = sse.ConnectionTimeout,
                UseStreamableHttp = sse.UseStreamableHttp
            }),
            _ => throw new NotSupportedException($"서버 타입 {server.GetType().Name}은(는) 지원되지 않습니다.")
        };
    }
}
