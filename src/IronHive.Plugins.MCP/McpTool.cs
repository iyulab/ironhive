using System.Data;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP (ModelContext Protocol) 클라이언트를 기반으로 한 도구 기능을 제공하는 클래스입니다.
/// </summary>
[JsonPolymorphicValue("mcp")]
public class McpTool : ITool
{
    private readonly McpClientTool _tool;
    
    public McpTool(McpClientTool tool)
    {
        _tool = tool ?? throw new ArgumentNullException(nameof(tool));
    }

    /// <summary>
    /// 현재 MCP 툴의 서버 이름입니다.
    /// </summary>
    public required string ServerName { get; init; }

    /// <inheritdoc />
    public string Name => $"mcp_{ServerName}_{_tool.Name}";

    /// <inheritdoc />
    public string Description => _tool.Description;

    /// <inheritdoc />
    public object? Parameters => _tool.JsonSchema;

    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = true;

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await _tool.CallAsync(
            arguments: input,
            progress: null,
            serializerOptions: null,
            cancellationToken: cancellationToken);

        var text = result.Content.Select(c => c.ToString());
        var content = string.Join("\n", text);
        
        return result.IsError.GetValueOrDefault(true)
            ? ToolOutput.Failure(content)
            : ToolOutput.Success(content);
    }
}
