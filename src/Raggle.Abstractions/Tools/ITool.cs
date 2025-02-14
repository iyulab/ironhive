using Raggle.Abstractions.Json;

namespace Raggle.Abstractions.Tools;

public interface ITool
{
    string Name { get; set; }
    string? Description { get; set; }
    IDictionary<string, JsonSchema>? Properties { get; set; }
    IEnumerable<string>? Required { get; set; }

    Task<ToolResult> InvokeAsync(ToolArguments? arguments);
}
