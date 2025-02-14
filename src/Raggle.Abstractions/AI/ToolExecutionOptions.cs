namespace Raggle.Abstractions.AI;

public enum ToolSelectionMode
{
    None,   // not use any tool
    Auto,   // use the tool automatically
    Manual,  // use the tool manually
    Sequence, // use the tool in sequence
}

public class ToolExecutionOptions
{
    /// <summary>
    /// the maximum number of tries to tool execution.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Tool calling choice
    /// </summary>
    public ToolSelectionMode SelectionMode { get; set; } = ToolSelectionMode.Auto;

    /// <summary>
    /// the list of tools to use.
    /// </summary>
    public IEnumerable<string>? AllowedTools { get; set; }
}
