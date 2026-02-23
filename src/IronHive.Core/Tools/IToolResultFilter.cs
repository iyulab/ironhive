using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <summary>
/// Filters tool execution results before they are returned to the LLM.
/// Applied in the MessageService tool execution loop to reduce token consumption.
/// </summary>
public interface IToolResultFilter
{
    /// <summary>
    /// Filters a tool execution result.
    /// </summary>
    /// <param name="toolName">The name of the tool that produced the result.</param>
    /// <param name="output">The original tool output.</param>
    /// <returns>The filtered tool output.</returns>
    ToolOutput Filter(string toolName, ToolOutput output);
}
