namespace IronHive.Abstractions.Tools;

/// <summary>
/// Defines an interface for a tool used to LLM Context.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    /// Gets or sets the JSON schema for the tool parameters.
    /// </summary>
    object? InputSchema { get; set; }

    /// <summary>
    /// Whether the tool requires confirmation before execution.
    /// </summary>
    bool RequiresApproval { get; set; }

    /// <summary>
    /// Invokes the tool asynchronously using the specified parameters.
    /// </summary>
    /// <param name="args">
    /// The parameters used to invoke the tool.
    /// </param>
    /// <returns>
    /// The result of the tool invocation.
    /// </returns>
    Task<ToolResult> InvokeAsync(object? args, CancellationToken cancellationToken = default);
}
