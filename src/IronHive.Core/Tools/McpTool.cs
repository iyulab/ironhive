using IronHive.Abstractions.Tools;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace IronHive.Core.Tools;

public class McpTool : ITool
{
    private readonly McpClientTool _client;

    public McpTool(McpClientTool client)
    {
        _client = client;
        Name = client.Name;
        Description = client.Description;
        InputSchema = client.JsonSchema;
    }

    /// <inheritdoc />
    public ToolPermission Permission { get; set; } = ToolPermission.Manual;

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public object? InputSchema { get; set; }

    /// <inheritdoc />
    public async Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default)
    {
        try
        {
            var dic = args.ConvertTo<IDictionary<string, object?>>();
            var arguments = new AIFunctionArguments(dic);
            var result = await _client.InvokeAsync(arguments, cancellationToken);
            return ToolResult.Success(result);
        }
        catch (Exception ex)
        {
            return ToolResult.Failed(ex);
        }
    }
}
