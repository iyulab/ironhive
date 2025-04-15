namespace IronHive.Abstractions.Tools;

public enum ToolPermission
{
    /// <summary>
    /// The tool is required to be called user confirmation.
    /// </summary>
    Manual,

    /// <summary>
    /// The tool is invoked automatically.
    /// </summary>
    Auto
}

/// <summary>
/// Defines an interface for a tool used to LLM Context.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets or sets the permission level of the tool.
    /// </summary>
    ToolPermission Permission { get; set; }

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
