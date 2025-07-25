using IronHive.Abstractions.Tools;
using ModelContextProtocol.Client;
using System.Collections.Concurrent;
using System.Data;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트를 기반으로 도구 호출 기능을 제공하는 플러그인 클래스입니다.
/// MCP 서버와의 통신을 통해 도구 목록을 조회하거나 도구를 실행하는 기능을 제공합니다.
/// </summary>
public class McpToolPlugin : IToolPlugin
{
    private readonly IMcpServerConfig _config;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private volatile bool _isDisposed;
    private IEnumerable<ToolDescriptor> _cache = [];
    
    private IMcpClient? _client;

    public McpToolPlugin(IMcpServerConfig server)
    {
        _config = server;

        // 서버 설정을 기반으로 MCP 클라이언트를 초기화합니다
        _ = Task.Run(async () => await ConnectAsync());
    }

    /// <inheritdoc />
    public required string PluginName { get; init; }

    /// <summary>
    /// 현재 MCP 서버와의 연결 상태를 나타냅니다.
    /// </summary>
    public McpConnectionState ConnectionState { get; private set; } = McpConnectionState.Disconnected;

    /// <summary>
    /// MCP 서버에 연결이 성공했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<ConnectedEventArgs>? Connected;

    /// <summary>
    /// MCP 서버와의 연결이 해제되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public event EventHandler<DisconnectedEventArgs>? Disconnected;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
        if (ConnectionState == McpConnectionState.Connected || ConnectionState == McpConnectionState.Connecting)
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // 기존 연결이 있다면 해제합니다.
            await DisconnectAsync();

            ConnectionState = McpConnectionState.Connecting;

            var transport = CreateTransport(_config);
            _client = await McpClientFactory.CreateAsync(
                clientTransport: transport,
                clientOptions: options,
                cancellationToken: cancellationToken);

            // 연결을 시도해 봅니다.
            await _client.PingAsync(cancellationToken: cancellationToken);

            ConnectionState = McpConnectionState.Connected;
            Connected?.Invoke(this, new ConnectedEventArgs(PluginName));
        }
        catch (Exception ex)
        {
            ConnectionState = McpConnectionState.Disconnected;
            Disconnected?.Invoke(this, new DisconnectedEventArgs(PluginName, ex));
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
        if (_client == null || ConnectionState == McpConnectionState.Disconnected)
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await _client.DisposeAsync();
            ConnectionState = McpConnectionState.Disconnected;
            Disconnected?.Invoke(this, new DisconnectedEventArgs(PluginName));
        }
        finally
        {
            _client = null;
            _cache = [];
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return Enumerable.Empty<ToolDescriptor>();

        if (_cache.Any())
            return _cache;

        var mcpTools = await _client.ListToolsAsync(cancellationToken: cancellationToken);
        var tools = mcpTools.Select(t => new ToolDescriptor
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.JsonSchema,
            RequiresApproval = true
        }).ToList();

        _cache = tools;
        return tools;
    }

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            throw new InvalidOperationException("MCP client is not initialized. Please connect to a server first.");

        var result = await _client.CallToolAsync(
            toolName: name,
            arguments: input,
            cancellationToken: cancellationToken);

        var text = result.Content.Select(c => c.Text?.Trim());
        var content = string.Join("\n", text);

        return result.IsError
            ? ToolOutput.Failure(content)
            : ToolOutput.Success(content);
    }

    /// <summary>
    /// 전송 계층을 생성합니다.
    /// </summary>
    private static IClientTransport CreateTransport(IMcpServerConfig server)
    {
        return server switch
        {
            McpStdioServerConfig stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = stdio.Command,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory
            }),
            McpSseServerConfig sse => new SseClientTransport(new SseClientTransportOptions
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
