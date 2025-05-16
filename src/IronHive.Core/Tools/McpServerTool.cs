using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace IronHive.Core.Tools;

public class McpServerTool : ITool
{
    private readonly McpClientTool _client;

    public McpServerTool(McpClientTool client)
    {
        _client = client;
        Name = client.Name;
        Description = client.Description;
        InputSchema = client.JsonSchema;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Description { get; set; }

    /// <inheritdoc />
    public object? InputSchema { get; set; }

    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = true;

    /// <inheritdoc />
    public async Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default)
    {
        try
        {
            var dic = args.ConvertTo<IDictionary<string, object?>>();
            var arguments = new AIFunctionArguments(dic);
            var obj = await _client.InvokeAsync(arguments, cancellationToken);
            var str = JsonSerializer.Serialize(obj, JsonDefaultOptions.Options);
            return ToolResult.Success(str);
        }
        catch (Exception ex)
        {
            return ToolResult.Failure(ex.Message);
        }
    }
}
