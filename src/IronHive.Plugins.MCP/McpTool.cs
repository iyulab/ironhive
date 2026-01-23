using System.Data;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using ModelContextProtocol.Client;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP(ModelContext Protocol) 클라이언트를 기반으로 동작하는 도구 구현체입니다.
/// 
/// 이 클래스는 IronHive 환경에서 MCP 서버와 통신하기 위한 Tool 역할을 수행합니다.
/// MCP 클라이언트를 감싸서(IronHive.Abstractions.Tools.ITool 인터페이스 구현)
/// IronHive의 표준화된 툴 호출 방식에 맞게 래핑(wrapping)합니다.
/// </summary>
[JsonPolymorphicValue("mcp")]
public class McpTool : ITool
{
    private readonly McpClientTool _tool;
    
    public McpTool(McpClientTool tool)
    {
        _tool = tool ?? throw new ArgumentNullException(nameof(tool));
    }

    /// <inheritdoc />
    public string UniqueName => $"mcp_{ServerName}_{Name}";

    /// <summary>
    /// 현재 MCP 툴이 속한 서버의 이름입니다.
    /// </summary>
    public required string ServerName { get; init; }

    /// <summary>
    /// MCP 툴의 이름입니다.
    /// </summary>
    public string Name => _tool.Name;

    /// <inheritdoc />
    public string? Description => _tool.Description;

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
            cancellationToken: cancellationToken);

        var text = result.Content.Select(c => c.ToString());
        var content = string.Join("\n", text);
        
        // TODO: MCP 호출 결과 확인 필요
        return result.IsError.GetValueOrDefault(true)
            ? ToolOutput.Failure(content)
            : ToolOutput.Success(content);
    }
}
