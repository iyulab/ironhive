namespace IronHive.Abstractions.Tools;

/// <summary>
/// Filters tool execution outputs before they are returned to the LLM.
/// Applied in the MessageService tool execution loop to reduce token consumption.
/// </summary>
public interface IToolOutputFilter
{
    /// <summary>
    /// Filters a tool execution output.
    /// </summary>
    /// <param name="toolName">The name of the tool that produced the output.</param>
    /// <param name="output">The original tool output.</param>
    /// <returns>The filtered tool output.</returns>
    ToolOutput Filter(string toolName, ToolOutput output);
}
