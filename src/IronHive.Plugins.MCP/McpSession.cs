using System.Data;
using IronHive.Plugins.MCP.Configurations;
using ModelContextProtocol.Client;
using ModelContextProtocol.Authentication;

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
        await DisconnectAsync().ConfigureAwait(false);
        _gate.Dispose();
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
            await _client.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (State == McpConnectionState.Connected && _client != null)
                return;

            // 클라이언트를 생성합니다.
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // 서버와의 연결을 확인합니다.
            await _client.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

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
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_client is not null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
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
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 기존 연결을 해제합니다.
            if (_client is not null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
                _client = null;
                UpdateState(McpConnectionState.Disconnected);
            }

            // 새로운 설정으로 클라이언트를 생성합니다.
            Config = config;
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // 서버와의 연결을 확인합니다.
            await _client.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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

        var tools = await _client.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return tools.Select(t =>
        {
            return new McpTool(t)
            {
                ServerName = ServerName
            };
        }).ToList();
    }

    /// <summary>
    /// 전송 계층을 생성합니다.
    /// </summary>
    private static IClientTransport CreateTransport(IMcpClientConfig config)
    {
        return config switch
        {
            McpStdioClientConfig stdio => new StdioClientTransport(BuildStdioOptions(stdio)),
            McpHttpClientConfig http => new HttpClientTransport(BuildHttpOptions(http)),
            _ => throw new NotSupportedException($"Server type {config.GetType().Name} is not supported.")
        };
    }

    /// <summary>
    /// Maps the stdio configuration onto the vendor transport options. Split out so the mapping can be
    /// asserted: the transport does not expose these once constructed, and the server name, the command
    /// and the working directory are three adjacent strings — a swap compiles and launches the wrong
    /// process, or launches the right one from the wrong place.
    /// </summary>
    internal static StdioClientTransportOptions BuildStdioOptions(McpStdioClientConfig stdio) => new()
    {
        Name = stdio.ServerName,
        Command = stdio.Command,
        Arguments = stdio.Arguments?.ToList(),
        EnvironmentVariables = stdio.EnvironmentVariables,
        ShutdownTimeout = stdio.ShutdownTimeout,
        WorkingDirectory = stdio.WorkingDirectory,
    };

    /// <summary>
    /// Maps the HTTP configuration onto the vendor transport options. The OAuth client id and secret are
    /// adjacent strings of the same type, so a swap compiles and sends the secret as the public
    /// identifier. The transport mode is adapter policy rather than configuration and is fixed here.
    /// </summary>
    internal static HttpClientTransportOptions BuildHttpOptions(McpHttpClientConfig http) => new()
    {
        TransportMode = HttpTransportMode.AutoDetect,
        Name = http.ServerName,
        Endpoint = http.Endpoint,
        AdditionalHeaders = http.AdditionalHeaders,
        ConnectionTimeout = http.ConnectionTimeout,
        OAuth = http.OAuth is { } oauth
            ? new ClientOAuthOptions
            {
                RedirectUri = oauth.RedirectUri,
                ClientId = oauth.ClientId,
                ClientSecret = oauth.ClientSecret,
                Scopes = oauth.Scopes,
                AdditionalAuthorizationParameters = oauth.AdditionalParameters ?? [],
            }
            : null,
    };

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
