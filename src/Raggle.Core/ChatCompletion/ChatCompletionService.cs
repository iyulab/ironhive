using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.ChatCompletion.Tools;
using System.Runtime.CompilerServices;

namespace Raggle.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly ITextParser<(string, string)> _parser;

    public ChatCompletionService(
        IHiveServiceContainer container,
        ITextParser<(string, string)> parser)
    {
        _connectors = container.GetKeyedServices<IChatCompletionConnector>();
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();
        foreach (var (key, connector) in _connectors)
        {
            var serviceModels = await connector.GetModelsAsync(cancellationToken);

            models.AddRange(serviceModels.Select(x => new ChatCompletionModel
            {
                Model = _parser.Stringify((key, x.Model)),
            }));
        }
        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResult<IMessage>> InvokeAsync(
        MessageContext context,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        options.Model = model;

        var count = 0;
        var messages = context.Messages.DeepCopy();
        EndReason? reason = null;
        IMessage? message = new AssistantMessage();
        TokenUsage? usage = new TokenUsage();
        while (count < context.MaxLoopCount)
        {
            var res = await connector.GenerateMessageAsync(messages, options, cancellationToken);
            reason = res.EndReason;

            if (reason == EndReason.ToolCall)
            {
                var tc = res.Data?.Content.OfType<ToolContent>() ?? [];
                foreach (var t in tc)
                {
                    if (options.Tools == null || string.IsNullOrEmpty(t.Name))
                        continue;

                    t.Result = options.Tools.TryGetValue(t.Name, out var tool)
                        ? await tool.InvokeAsync(t.Arguments)
                        : ToolResult.Failed($"Tool '{t.Name}' not found.");
                }
                message = res.Data;
            }
            else if (reason == EndReason.MaxTokens)
            {

            }
            else
            {

            }
        }

        return new ChatCompletionResult<IMessage>
        {
            EndReason = EndReason.ToolFailed,
            Data = message,
            TokenUsage = usage,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> InvokeStreamingAsync(
        MessageContext context,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        options.Model = model;

        await foreach (var res in connector.GenerateStreamingMessageAsync(context.Messages, options, cancellationToken))
        {
            if (res.EndReason == EndReason.ToolCall)
            {
                yield return res;
            }
            else if (res.EndReason == EndReason.MaxTokens)
            {
                yield return res;
            }
            else
            {
                yield return res;
            }
        }
    }
}
