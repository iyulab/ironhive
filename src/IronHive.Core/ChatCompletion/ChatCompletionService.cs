using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly IModelParser _parser;
    private readonly IMessageReducer _reducer;

    public ChatCompletionService(
        IHiveServiceContainer container,
        //IMessageReducer reducer,
        IModelParser parser)
    {
        _connectors = container.GetKeyedServices<IChatCompletionConnector>();
        _parser = parser;
        //_reducer = reducer;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();
        foreach (var (key, connector) in _connectors)
        {
            var sModels = await connector.GetModelsAsync(cancellationToken);
            models.AddRange(sModels.Select(x => new ChatCompletionModel
            {
                Model = _parser.Stringify((key, x.Model)),
            }));
        }
        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResult<Message>> InvokeAsync(
        MessageContext context,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        options.Model = model;

        bool doLoop = true;
        var messages = context.Messages.Clone();

        while (doLoop)
        {
            var res = await connector.GenerateMessageAsync(messages, options, cancellationToken);

            if (res.EndReason == EndReason.ToolCall)
            {
                var toolContents = res.Data?.Content.OfType<ToolContent>() ?? [];
                foreach (var content in toolContents)
                {
                    if (options.Tools == null || string.IsNullOrEmpty(content.Name))
                        continue;
                    if (options.Tools.TryGetValue(content.Name, out var tool))
                    {
                        var args = new ToolArguments(content.Arguments);
                        var result = await tool.InvokeAsync(args);
                        content.Status = result.IsSuccess ? ToolStatus.Completed : ToolStatus.Failed;
                        content.Result = result.ToString();
                    }
                    else
                    {
                        content.Status = ToolStatus.Failed;
                        content.Result = $"Tool '{content.Name}' not found.";
                    }
                }

                if (res.Data == null)
                    throw new InvalidOperationException("Tool call end reason without data.");

                messages.Add(res.Data);
                doLoop = true;
            }
            else if (res.EndReason == EndReason.MaxTokens)
            {
                doLoop = true;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
            }
            else
            {
                doLoop = false;
                return new ChatCompletionResult<Message>
                {
                    EndReason = res.EndReason,
                    Data = res.Data,
                    TokenUsage = res.TokenUsage,
                };
            }
        }

        throw new InvalidOperationException("Unexpected end of loop.");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> InvokeStreamingAsync(
        MessageContext context,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        #region Tool Test
        options.Tools ??= new ToolCollection();
        var tt = FunctionToolFactory.CreateFromObject(this);
        options.Tools.AddRange(tt);
        #endregion

        var (serviceKey, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        options.Model = model;

        bool doLoop = true;
        var messages = context.Messages.Clone();

        while (doLoop)
        {
            var message = new Message(MessageRole.Assistant);
            EndReason? reason = null;
            TokenUsage? usage = null;
            await foreach (var res in connector.GenerateStreamingMessageAsync(messages, options, cancellationToken))
            {
                yield return res;

                if (res.Data != null)
                {
                    if (res.Data is TextContent text)
                    {
                        if (message.Content.LastOrDefault() is TextContent last)
                            last.Value += text.Value;
                        else
                            message.Content.Add(text);
                    }

                    if (res.Data is ToolContent tool)
                    {
                        if (tool.Index != null && message.Content.TryGetAt<ToolContent>((int)tool.Index, out var last))
                            last.Arguments += tool.Arguments;
                        else
                            message.Content.Add(tool);
                    }
                }
                if (res.EndReason != null)
                {
                    reason = res.EndReason;
                }
                if (res.TokenUsage != null)
                {
                    usage = res.TokenUsage;
                }
            }

            if (reason == EndReason.ToolCall)
            {
                var toolContents = message.Content.OfType<ToolContent>();
                foreach (var content in toolContents)
                {
                    if (content.Name == null || options.Tools == null)
                        continue;

                    if (options.Tools.TryGetValue(content.Name, out var tool))
                    {
                        content.Status = ToolStatus.Running;
                        yield return new ChatCompletionResult<IMessageContent>
                        {
                            Data = content,
                        };

                        var args = new ToolArguments(content.Arguments);
                        var result = await tool.InvokeAsync(args);
                        content.Status = result.IsSuccess ? ToolStatus.Completed : ToolStatus.Failed;
                        content.Result = result.ToString();

                        yield return new ChatCompletionResult<IMessageContent>
                        {
                            Data = content,
                        };
                    }
                    else
                    {
                        content.Status = ToolStatus.Failed;
                        content.Result = $"Tool '{content.Name}' not found.";

                        yield return new ChatCompletionResult<IMessageContent>
                        {
                            Data = content,
                        };
                    }

                    messages.Add(message);
                    doLoop = true;
                }
            }
            else if (reason == EndReason.MaxTokens)
            {
                doLoop = true;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
            }
            else
            {
                doLoop = false;
            }
        }
    }

    [FunctionTool("date-previous")]
    public DateTime GetPreviousTime(int days)
    {
        return DateTime.UtcNow.AddDays(-days);
    }

    [FunctionTool("date-now")]
    public DateTime GetNow()
    {
        return DateTime.UtcNow;
    }

    [FunctionTool("date-next")]
    public DateTime GetNextTime(int days)
    {
        return DateTime.UtcNow.AddDays(days);
    }
}
