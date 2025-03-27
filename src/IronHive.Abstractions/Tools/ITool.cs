using IronHive.Abstractions.Json;

namespace IronHive.Abstractions.Tools;

public interface ITool
{
    /// <summary>
    /// 
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    ObjectJsonSchema? Parameters { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default);
}
