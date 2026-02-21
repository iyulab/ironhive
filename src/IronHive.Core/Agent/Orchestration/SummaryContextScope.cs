using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// Context scope that creates a structured summary of the conversation history
/// and combines it with the current task. Bridges the gap between full context
/// (too expensive) and task-only (too little context).
/// </summary>
/// <remarks>
/// The summary extracts: original goal, tool actions performed, file paths
/// mentioned, and error codes encountered. Sub-agents receive enough context
/// to understand what has happened without the full message history.
/// </remarks>
public sealed partial class SummaryContextScope : IContextScope
{
    private readonly SummaryContextScopeOptions _options;

    /// <summary>
    /// Creates a summary context scope with the specified options.
    /// </summary>
    public SummaryContextScope(SummaryContextScopeOptions? options = null)
    {
        _options = options ?? new SummaryContextScopeOptions();
    }

    /// <inheritdoc />
    public IReadOnlyList<Message> ScopeMessages(
        IReadOnlyList<Message> messages,
        string agentName)
    {
        if (messages.Count <= _options.MinMessagesForSummary)
        {
            return messages;
        }

        var summary = ExtractSummary(messages);
        var lastTask = FindLastUserMessage(messages);

        var result = new List<Message>();

        // Add summary as a user message with context block
        if (summary.HasContent)
        {
            var summaryText = summary.Format(_options.MaxGoalLength);
            result.Add(new UserMessage
            {
                Content = [new TextMessageContent { Value = summaryText }]
            });
        }

        // Add the current task (last user message)
        if (lastTask is not null)
        {
            // Avoid duplicate if the last user message IS the only message
            if (!summary.HasContent || !ReferenceEquals(lastTask, messages[0]))
            {
                result.Add(lastTask);
            }
        }

        // Fallback: if we somehow have nothing, return original
        return result.Count > 0 ? result : messages;
    }

    #region Summary Extraction

    private static ConversationSummary ExtractSummary(IReadOnlyList<Message> messages)
    {
        var summary = new ConversationSummary();
        var goalFound = false;

        foreach (var message in messages)
        {
            // Extract goal from first user message
            if (message is UserMessage firstUser && !goalFound)
            {
                var text = GetTextContent(firstUser.Content);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    summary.Goal = text;
                    goalFound = true;
                }
            }

            // Extract tool calls and file paths from assistant messages
            if (message is AssistantMessage assistant)
            {
                foreach (var content in assistant.Content)
                {
                    if (content is ToolMessageContent tool)
                    {
                        ExtractToolInfo(tool, summary);
                    }
                }
            }

            // Extract error codes from all text content
            ExtractErrorCodes(message, summary);
        }

        return summary;
    }

    private static void ExtractToolInfo(ToolMessageContent tool, ConversationSummary summary)
    {
        if (string.IsNullOrEmpty(tool.Name))
        {
            return;
        }

        // Track tool usage (name + key parameter)
        var toolAction = FormatToolAction(tool);
        summary.ToolActions.Add(toolAction);

        // Track file paths from file-modifying tools
        if (IsFileModifyingTool(tool.Name) && tool.Input is not null)
        {
            var path = ExtractPathFromInput(tool.Input);
            if (path is not null)
            {
                summary.FilesModified.Add(path);
            }
        }
    }

    private static string FormatToolAction(ToolMessageContent tool)
    {
        if (tool.Input is null)
        {
            return tool.Name;
        }

        var keyParam = ExtractKeyParameter(tool.Input);
        return keyParam is not null
            ? $"{tool.Name}({keyParam})"
            : tool.Name;
    }

    private static string? ExtractKeyParameter(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            // Try common parameter names in priority order
            // Search-oriented params first (pattern/query), then path-oriented
            foreach (var paramName in new[] { "pattern", "query", "path", "file_path", "filePath", "command", "url" })
            {
                if (root.TryGetProperty(paramName, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    var str = value.GetString();
                    if (str is not null && str.Length > 60)
                    {
                        str = string.Concat(str.AsSpan(0, 57), "...");
                    }
                    return str;
                }
            }
        }
        catch (JsonException)
        {
            // Input is not valid JSON — ignore
        }

        return null;
    }

    private static string? ExtractPathFromInput(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var paramName in new[] { "path", "file_path", "filePath" })
            {
                if (root.TryGetProperty(paramName, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }
        }
        catch (JsonException)
        {
            // Ignore
        }

        return null;
    }

    private static bool IsFileModifyingTool(string toolName)
    {
        return toolName.Contains("write", StringComparison.OrdinalIgnoreCase) ||
               toolName.Contains("edit", StringComparison.OrdinalIgnoreCase) ||
               toolName.Contains("create", StringComparison.OrdinalIgnoreCase) ||
               toolName.Contains("delete", StringComparison.OrdinalIgnoreCase);
    }

    private static void ExtractErrorCodes(Message message, ConversationSummary summary)
    {
        ICollection<MessageContent>? contents = message switch
        {
            UserMessage u => u.Content,
            AssistantMessage a => a.Content,
            _ => null
        };

        if (contents is null) return;

        foreach (var content in contents)
        {
            if (content is TextMessageContent text && !string.IsNullOrEmpty(text.Value))
            {
                foreach (Match match in ErrorCodePattern().Matches(text.Value))
                {
                    summary.ErrorCodes.Add(match.Value);
                }
            }
        }
    }

    [GeneratedRegex(@"(?:CS|CA|IDE|SA)\d{4,5}")]
    private static partial Regex ErrorCodePattern();

    #endregion

    #region Helpers

    private static UserMessage? FindLastUserMessage(IReadOnlyList<Message> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i] is UserMessage user)
            {
                return user;
            }
        }

        return null;
    }

    private static string GetTextContent(ICollection<MessageContent> content)
    {
        foreach (var item in content)
        {
            if (item is TextMessageContent text && !string.IsNullOrEmpty(text.Value))
            {
                return text.Value;
            }
        }

        return string.Empty;
    }

    #endregion
}

/// <summary>
/// Options for <see cref="SummaryContextScope"/>.
/// </summary>
public sealed class SummaryContextScopeOptions
{
    /// <summary>
    /// Minimum number of messages before summarization kicks in.
    /// Below this threshold, all messages are passed through unchanged.
    /// Default: 4.
    /// </summary>
    public int MinMessagesForSummary { get; init; } = 4;

    /// <summary>
    /// Maximum character length for the goal in the summary.
    /// Default: 200.
    /// </summary>
    public int MaxGoalLength { get; init; } = 200;
}

/// <summary>
/// Structured summary of a conversation extracted by <see cref="SummaryContextScope"/>.
/// </summary>
internal sealed class ConversationSummary
{
    public string? Goal { get; set; }
    public List<string> ToolActions { get; } = [];
    public HashSet<string> FilesModified { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ErrorCodes { get; } = new(StringComparer.Ordinal);

    public bool HasContent =>
        Goal is not null ||
        ToolActions.Count > 0 ||
        FilesModified.Count > 0 ||
        ErrorCodes.Count > 0;

    public string Format(int maxGoalLength)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Context Summary]");

        if (Goal is not null)
        {
            var truncatedGoal = Goal.Length > maxGoalLength
                ? string.Concat(Goal.AsSpan(0, maxGoalLength - 3), "...")
                : Goal;
            sb.Append("Goal: ").AppendLine(truncatedGoal);
        }

        if (ToolActions.Count > 0)
        {
            // Deduplicate and limit to recent actions
            var recent = ToolActions
                .Distinct(StringComparer.Ordinal)
                .TakeLast(20)
                .ToList();
            sb.Append("Actions: ").AppendLine(string.Join(", ", recent));
        }

        if (FilesModified.Count > 0)
        {
            sb.Append("Files modified: ").AppendLine(string.Join(", ", FilesModified.Order()));
        }

        if (ErrorCodes.Count > 0)
        {
            sb.Append("Errors: ").AppendLine(string.Join(", ", ErrorCodes.Order()));
        }

        sb.Append("[End Summary]");
        return sb.ToString();
    }
}
