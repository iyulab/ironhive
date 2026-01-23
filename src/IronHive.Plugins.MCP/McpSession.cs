using System.Data;
using ModelContextProtocol.Client;
using IronHive.Plugins.MCP.Configurations;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트를 기반으로 서버와의 연결을 관리합니다.
/// MCP 서버와의 통신을 통해 도구 목록을 조회하거나 도구를 실행하는 기능을 제공합니다.
/// </summary>
public class McpSession : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private McpClient? _client;

    public McpSession(IMcpClientConfig config)
    {
        ServerName = config.ServerName;
        Config = config;
    }

    /// <summary>
    /// 현재 연결된 서버의 이름을 나타냅니다.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// 현재 MCP 서버와의 연결 상태를 나타냅니다.
    /// </summary>
    public McpConnectionState State { get; private set; } = McpConnectionState.Disconnected;

    /// <summary>
    /// 현재 연결된 서버와의 설정을 나타냅니다.
    /// </summary>
    public IMcpClientConfig Config { get; private set; }

    /// <summary>
    /// MCP 서버와의 연결중 오류가 발생했을 때의 오류 메시지를 나타냅니다.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// MCP 서버에 연결이 성공했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<McpConnectionEventArgs>? Connected;

    /// <summary>
    /// MCP 서버와의 연결이 해제되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<McpConnectionEventArgs>? Disconnected;

    /// <summary>
    /// MCP 서버와의 연결 중 오류가 발생했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<McpErroredEventArgs>? Errored;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        Connected = null;
        Disconnected = null;
        Errored = null;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// MCP 서버와의 연결을 확인합니다.
    /// </summary>
    public async Task<bool> HealthAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return false;

        try
        {
            await _client.PingAsync(cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            UpdateState(McpConnectionState.Errored, ex);
            return false;
        }
    }

    /// <summary>
    /// MCP 서버에 연결합니다. 기존 연결이 있다면, 해당 연결을 유지합니다.
    /// </summary>
    public async Task ConnectAsync(
        McpClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (State == McpConnectionState.Connected && _client != null)
                return;

            // 클라이언트를 생성합니다.
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken);

            // 서버와의 연결을 확인합니다.
            await _client.PingAsync(cancellationToken: cancellationToken);

            UpdateState(McpConnectionState.Connected);
        }
        catch (Exception ex)
        {
            UpdateState(McpConnectionState.Errored, ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// MCP 서버와의 연결을 해제합니다.
    /// </summary>
    public async Task DisconnectAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_client is not null)
            {
                await _client.DisposeAsync();
                _client = null;
            }
            
            UpdateState(McpConnectionState.Disconnected);
        }
        catch (Exception ex)
        {
            UpdateState(McpConnectionState.Errored, ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 새로운 연결을 시도합니다. 기존 연결이 있다면, 해당 연결을 해제하고 새로운 연결을 생성합니다.
    /// </summary>
    public async Task ReconnectAsync(
        IMcpClientConfig config,
        McpClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            // 기존 연결을 해제합니다.
            if (_client is not null)
            {
                await _client.DisposeAsync();
                _client = null;
                UpdateState(McpConnectionState.Disconnected);
            }

            // 새로운 설정으로 클라이언트를 생성합니다.
            Config = config;
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken);
            
            // 서버와의 연결을 확인합니다.
            await _client.PingAsync(cancellationToken: cancellationToken);
            UpdateState(McpConnectionState.Connected);
        }
        catch (Exception ex)
        {
            UpdateState(McpConnectionState.Errored, ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Mcp 서버의 툴을 반환합니다.
    /// </summary>
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
                ServerName = ServerName
            };
        });
    }

    /// <summary>
    /// 전송 계층을 생성합니다.
    /// </summary>
    private static IClientTransport CreateTransport(IMcpClientConfig config)
    {
        return config switch
        {
            McpStdioClientConfig stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = stdio.ServerName,
                Command = stdio.Command,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory,
                //StandardErrorLines = (str) => Console.Error.WriteLine(str),
            }),
            McpSseClientConfig sse => new HttpClientTransport(new HttpClientTransportOptions
            {
                Name = sse.ServerName,
                Endpoint = sse.Endpoint,
                AdditionalHeaders = sse.AdditionalHeaders,
                ConnectionTimeout = sse.ConnectionTimeout,
            }),
            _ => throw new NotSupportedException($"서버 타입 {config.GetType().Name}은(는) 지원되지 않습니다.")
        };
    }

    /// <summary>
    /// 현재 세션의 상태를 업데이트 합니다.
    /// </summary>
    private void UpdateState(McpConnectionState state, Exception? ex = null)
    {
        State = state;
        ErrorMessage = ex?.Message;
        
        if (state == McpConnectionState.Connected)
        {
            Connected?.Invoke(this, new McpConnectionEventArgs(ServerName));
        }
        else if (state == McpConnectionState.Disconnected)
        {
            Disconnected?.Invoke(this, new McpConnectionEventArgs(ServerName));
        }
        else if (state == McpConnectionState.Errored)
        {
            Errored?.Invoke(this, new McpErroredEventArgs(ServerName, ex));
        }
    }
}
