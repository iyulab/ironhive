using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Core.Experiment;

public enum CondensationStrategy
{
    None,
    Truncate,
    Summarize
}

public class MessageSession
{
    private readonly IChatCompletionService _service;

    public CondensationStrategy Strategy { get; set; } = CondensationStrategy.None;

    public int CurrentToolAttempts {  get; private set; } = 0;
    public int MaxRetryToolAttempts { get; set; } = 3;

    public MessageSession(IChatCompletionService service)
    {
        _service = service;
    }

    public async Task<IMessage> InvokeAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var res = await _service.GenerateMessageAsync(request, cancellationToken);
        if (res.EndReason == ChatCompletionEndReason.MaxTokens) 
        {
            return res.Content;
        }
        else if (res.EndReason == ChatCompletionEndReason.ToolUse)
        {
            var isFailed = false;
            foreach (var item in res.Content?.Content)
            {
                if (item is ToolContent tool)
                {
                    if(request.Tools.TryGetValue(tool.Name, out var toolInfo))
                    {
                        tool.Result = await toolInfo.InvokeAsync(tool.Arguments);
                    }
                    else
                    {
                        isFailed = true;
                    }
                }
            }

            if (isFailed)
            {
                if (CurrentToolAttempts < MaxRetryToolAttempts)
                {
                    CurrentToolAttempts++;
                    return await InvokeAsync(request, cancellationToken);
                }
            }
        }

        return res.Content;
    }

    public async IAsyncEnumerable<IMessageContent> InvokeStreamingAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        await foreach (var res in _service.GenerateStreamingMessageAsync(request, cancellationToken))
        {
            if (res.EndReason == ChatCompletionEndReason.MaxTokens)
            {
                yield return res.Content;
            }
            else if (res.EndReason == ChatCompletionEndReason.ToolUse)
            {
                yield return res.Content;
            }
            else
            {
                yield return res.Content;
            }
        }
    }
}
