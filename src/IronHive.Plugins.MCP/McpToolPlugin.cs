using IronHive.Abstractions.Tools;
using ModelContextProtocol.Client;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트를 기반으로 도구 호출 기능을 제공하는 플러그인 클래스입니다.
/// MCP 서버와의 통신을 통해 도구 목록을 조회하거나 도구를 실행하는 기능을 제공합니다.
/// </summary>
public class McpToolPlugin : IToolPlugin
{
    private IMcpClient _client;

    public McpToolPlugin(IMcpClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public required string PluginName { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ToolDescriptor>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tools = await _client.ListToolsAsync(
            cancellationToken: cancellationToken);

        return tools.Select(t => new ToolDescriptor
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.JsonSchema,
            RequiresApproval = true
        });
    }

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        string name,
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await _client.CallToolAsync(
            toolName: name,
            arguments: input,
            cancellationToken: cancellationToken);

        var text = result.Content.Select(c => c.Text?.Trim());
        var content = string.Join("\n", text);
        if (result.IsError)
        {
            return ToolOutput.Failure(content);
        }
        else
        {
            return ToolOutput.Success(content);
        }
    }
}
