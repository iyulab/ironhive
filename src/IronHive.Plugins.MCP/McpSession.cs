using System.Data;
using System.Text.Json.Nodes;
using IronHive.Plugins.MCP.Configurations;
using ModelContextProtocol.Client;
using ModelContextProtocol.Authentication;
using ModelContextProtocol.Protocol;

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
    /// 이 세션에서 협상된 MCP 프로토콜 리비전(예: <c>2026-07-28</c>, <c>2025-11-25</c>)입니다.
    /// 연결 전에는 <see langword="null"/>입니다. SDK가 <c>server/discover</c>로 먼저 협상하고,
    /// 그 리비전 이전의 서버에만 <c>initialize</c> 핸드셰이크로 폴백합니다.
    /// </summary>
    public string? NegotiatedProtocolVersion => _client?.NegotiatedProtocolVersion;

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
    /// MCP 서버가 «협상된 리비전의 스펙 기준으로» 살아 있는지 확인합니다. 2026-07-28 리비전은 <c>ping</c>을
    /// 제거하고 <c>server/discover</c>를 필수 RPC로 두었으므로, 그 리비전 이상으로 협상된 세션은
    /// <c>server/discover</c>로, 그 이전(<c>initialize</c> 핸드셰이크) 세션은 <c>ping</c>으로 묻습니다.
    /// 그 요청에 응답하지 않는 서버는 여기서 <see cref="McpConnectionState.Errored"/>가 된다 — 명시적으로
    /// «스펙 기준 생존 여부»를 물은 호출자에게는 그것이 답이다. 연결 자체는 이 검사를 전제하지 않는다
    /// (<see cref="ConnectAsync"/> 참조).
    /// </summary>
    public async Task<bool> HealthAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return false;

        try
        {
            if (UsesDiscoverLiveness(_client.NegotiatedProtocolVersion))
            {
                // The 2026-07-28 revision removed ping; server/discover is the mandatory utility.
                await _client.SendRequestAsync(
                    new JsonRpcRequest { Method = RequestMethods.ServerDiscover, Params = new JsonObject() },
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _client.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
        catch (Exception ex)
        {
            UpdateState(McpConnectionState.Errored, ex);
            return false;
        }
    }

    /// <summary>
    /// 협상된 리비전이 2026-07-28 이상이면 <c>server/discover</c>가 생존 검사 수단이다. 리비전 문자열은
    /// ISO 날짜라 서수 비교로 순서가 선다. 협상 전(<see langword="null"/>)은 <c>ping</c> 경로로 둔다.
    /// </summary>
    internal static bool UsesDiscoverLiveness(string? negotiatedProtocolVersion) =>
        negotiatedProtocolVersion is not null
        && string.CompareOrdinal(negotiatedProtocolVersion, July2026ProtocolVersion) >= 0;

    private const string July2026ProtocolVersion = "2026-07-28";

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

            // 클라이언트를 생성합니다. CreateAsync는 initialize/initialized 핸드셰이크가 끝나야 반환하므로
            // 여기까지 왔다는 것이 곧 서버가 요청에 응답한다는 증명이다. 그 직후에 다시 ping을 보내는 것은
            // 생존 정보를 더하지 않으면서, ping을 구현하지 않은 서버 하나를 «도구 전부 소실 + 상태 플래그 하나»로
            // 바꾸는 유일한 경로였다(McpClientManager는 Connected 이벤트에서만 도구를 등록한다). 스펙 기준 생존
            // 검사가 필요하면 HealthAsync를 명시적으로 부른다.
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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

            // 새로운 설정으로 클라이언트를 생성합니다. (ConnectAsync와 같은 이유로 핸드셰이크 뒤 ping은 없다.)
            Config = config;
            var transport = CreateTransport(Config);
            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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
    /// Mcp 서버의 툴을 반환합니다. 조회가 실패하면 세션을 <see cref="McpConnectionState.Errored"/>로 옮기고
    /// <see cref="Errored"/>를 발생시킨 뒤 예외를 다시 던진다 — 도구를 나열하지 못하는 서버는 연결에 실패한
    /// 서버와 똑같이 소비자에게 아무것도 주지 못하므로, 같은 경로로 드러나야 한다(조용히 «연결됨 + 도구 0개»로
    /// 남는 것이 가장 나쁜 결과다).
    /// </summary>
    public async Task<IEnumerable<McpTool>> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return [];

        IList<McpClientTool> tools;
        try
        {
            tools = await _client.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            UpdateState(McpConnectionState.Errored, ex);
            throw;
        }

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
