using ModelContextProtocol.Client;

namespace IronHive.Plugins.MCP;

public static class McpToolPuginFactory
{
    /// <summary>
    /// MCP (ModelContext Protocol) 클라이언트를 기반으로 도구 호출 기능을 제공하는 플러그인 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="name">플러그인 이름</param>
    /// <param name="server">MCP 서버 구현체 (Stdio 또는 SSE 방식)</param>
    /// <returns>McpToolPlugin 인스턴스</returns>
    /// <exception cref="NotSupportedException">지원하지 않는 서버 타입일 경우 발생</exception>
    /// <exception cref="InvalidOperationException">클라이언트 생성에 실패한 경우 발생</exception>
    public static McpToolPlugin Create(string name, IMcpServer server)
    {
        IClientTransport transport;
        if (server is McpStdioServer stdio)
        {
            transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = name,
                Command = stdio.Command,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables,
                ShutdownTimeout = stdio.ShutdownTimeout,
                WorkingDirectory = stdio.WorkingDirectory
            });
        }
        else if (server is McpSseServer sse)
        {
            transport = new SseClientTransport(new SseClientTransportOptions
            {
                Name = name,
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

        var client = McpClientFactory.CreateAsync(
            clientTransport: transport,
            clientOptions: null)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (client == null)
            throw new InvalidOperationException("Failed to create MCP client.");

        return new McpToolPlugin(client)
        { 
            PluginName = name 
        };
    }
}
