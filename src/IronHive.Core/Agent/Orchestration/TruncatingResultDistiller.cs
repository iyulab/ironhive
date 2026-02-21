using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// A result distiller that truncates verbose text content while preserving
/// tool call information and structure. Does not require an LLM — uses
/// simple head+tail truncation with a summary of omitted content.
/// </summary>
public sealed class TruncatingResultDistiller : IResultDistiller
{
    /// <inheritdoc />
    public Task<MessageResponse> DistillAsync(
        string agentName,
        MessageResponse response,
        ResultDistillationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ResultDistillationOptions();

        var assistant = response.Message;
        var totalChars = GetTotalTextChars(assistant.Content);

        // Skip distillation for short results
        if (totalChars < options.MinInputCharsForDistillation)
        {
            return Task.FromResult(response);
        }

        var distilledContent = DistillContent(assistant.Content, options);

        var distilledMessage = new AssistantMessage
        {
            Name = assistant.Name,
            Model = assistant.Model,
            Content = distilledContent
        };

        var distilledResponse = new MessageResponse
        {
            Id = response.Id,
            DoneReason = response.DoneReason,
            Message = distilledMessage,
            TokenUsage = response.TokenUsage
        };

        return Task.FromResult(distilledResponse);
    }

    private static List<MessageContent> DistillContent(
        ICollection<MessageContent> content,
        ResultDistillationOptions options)
    {
        var result = new List<MessageContent>();
        var textBudget = options.MaxOutputChars;

        foreach (var item in content)
        {
            switch (item)
            {
                case ToolMessageContent tool when options.PreserveToolCalls:
                    // Preserve tool calls but truncate their output
                    result.Add(TruncateToolContent(tool, Math.Max(500, textBudget / 4)));
                    break;

                case ToolMessageContent when !options.PreserveToolCalls:
                    // Skip tool content entirely
                    break;

                case TextMessageContent text:
                    if (textBudget > 0)
                    {
                        var truncated = TruncateText(text.Value, textBudget);
                        result.Add(new TextMessageContent { Value = truncated });
                        textBudget -= truncated.Length;
                    }
                    break;

                default:
                    // Pass through other content types (thinking, etc.)
                    result.Add(item);
                    break;
            }
        }

        return result;
    }

    private static ToolMessageContent TruncateToolContent(ToolMessageContent tool, int maxOutputChars)
    {
        if (tool.Output is null)
        {
            return tool;
        }

        var outputText = tool.Output.Result;
        if (outputText is null || outputText.Length <= maxOutputChars)
        {
            return tool;
        }

        var truncated = TruncateText(outputText, maxOutputChars);

        return new ToolMessageContent
        {
            Id = tool.Id,
            Name = tool.Name,
            Input = tool.Input,
            IsApproved = tool.IsApproved,
            Output = new Abstractions.Tools.ToolOutput
            {
                Result = truncated,
                IsSuccess = tool.Output.IsSuccess
            }
        };
    }

    private static string TruncateText(string text, int maxChars)
    {
        if (text.Length <= maxChars)
        {
            return text;
        }

        // Head + tail truncation with context
        var headChars = maxChars * 2 / 3;
        var tailChars = maxChars - headChars - 50; // Reserve space for truncation marker

        if (tailChars < 50)
        {
            // If budget is too small for head+tail, just take head
            return string.Concat(text.AsSpan(0, maxChars - 30), "\n\n[... truncated ...]");
        }

        var omittedChars = text.Length - headChars - tailChars;
        return string.Concat(
            text.AsSpan(0, headChars),
            $"\n\n[... {omittedChars:N0} chars omitted ...]\n\n",
            text.AsSpan(text.Length - tailChars));
    }

    private static int GetTotalTextChars(ICollection<MessageContent> content)
    {
        var total = 0;
        foreach (var item in content)
        {
            total += item switch
            {
                TextMessageContent text => text.Value.Length,
                ToolMessageContent tool => (tool.Output?.Result?.Length ?? 0) + (tool.Input?.Length ?? 0),
                _ => 0
            };
        }
        return total;
    }
}
