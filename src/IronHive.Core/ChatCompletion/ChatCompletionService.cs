using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;
using IronHive.Abstractions.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly IReadOnlyDictionary<string, FunctionToolCollection> _tools;
    private readonly IServiceModelParser _parser;

    public ChatCompletionService(
        IReadOnlyDictionary<string, IChatCompletionConnector> connectors,
        IReadOnlyDictionary<string, FunctionToolCollection> tools,
        IServiceModelParser parser)
    {
        _connectors = connectors;
        _tools = tools;
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();
        foreach (var (key, conn) in _connectors)
        {
            var sModels = await conn.GetModelsAsync(cancellationToken);
            models.AddRange(sModels.Select(x => new ChatCompletionModel
            {
                Model = _parser.Stringify((key, x.Model)),
            }));
        }
        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionModel> GetModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var (key, id) = _parser.Parse(model);
        if (!_connectors.TryGetValue(key, out var conn))
            throw new KeyNotFoundException($"Service key '{key}' not found.");

        return await conn.GetModelAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> ExecuteAsync(
        MessageSession session,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var (key, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(key, out var conn))
            throw new KeyNotFoundException($"Service key '{key}' not found.");
        options.Model = model;

        if (!string.IsNullOrEmpty(session.Summary))
        {
            options.System = $"\n### Conversation Summary\n{session.Summary}\n";
        }

        int failedCount = 0;
        var messages = session.Messages.Clone();
        var message = new Message(MessageRole.Assistant);

        while (failedCount < session.MaxToolAttempts)
        {
            var res = await conn.GenerateMessageAsync(messages, options, cancellationToken);

            // 메시지 정리 및 재시도
            if (res.EndReason == EndReason.MaxTokens)
            {
                session.LastTruncatedIndex = messages.Count - 1;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
                continue;
            }

            // 토큰 사용량 업데이트
            if (res.TokenUsage != null)
            {
                session.TotalTokens = res.TokenUsage.TotalTokens;
            }

            // 메시지 컨텐츠 추가
            message.Content.AddRange(res.Data?.Content ?? []);

            // 메시지 추가
            if (messages.LastOrDefault()?.Role != MessageRole.Assistant)
            {
                messages.Add(message);
            }

            // 도구 호출
            if (res.EndReason == EndReason.ToolCall)
            {
                var toolGroup = message.Content.OfType<ToolContent>() ?? [];
                foreach (var content in toolGroup)
                {
                    if (options.Tools == null || string.IsNullOrEmpty(content.Name))
                        continue;
                    if (options.Tools.TryGetValue(content.Name, out var tool))
                    {
                        var result = await tool.InvokeAsync(content.Arguments);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                        if (!result.IsSuccess)
                            content.FailedError(data);
                        else if (data.Length > 20_000)
                            content.FailedLargeResult();
                        else
                            content.Completed(data);
                            
                    }
                    else
                    {
                        content.FailedNotFound();
                    }
                }

                if (toolGroup.Any(x => x.Status == ToolStatus.Failed))
                {
                    failedCount++;
                }
            }
            // 종료
            else
            {
                break;
            }
        }

        return message;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IMessageContent> ExecuteStreamingAsync(
        MessageSession session,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (key, model) = _parser.Parse(options.Model);
        if (!_connectors.TryGetValue(key, out var conn))
            throw new KeyNotFoundException($"Service key '{key}' not found.");
        options.Model = model;

        if(!string.IsNullOrEmpty(session.Summary))
        {
            options.System = $"\n### Conversation Summary\n{session.Summary}\n";
        }

        int failedCount = 0;
        var messages = session.Messages.Clone();
        var message = new Message(MessageRole.Assistant);

        while (failedCount < session.MaxToolAttempts)
        {
            var stack = new MessageContentCollection();
            EndReason? reason = null;
            TokenUsage? usage = null;
            
            // 메시지 생성
            await foreach (var res in conn.GenerateStreamingMessageAsync(messages, options, cancellationToken))
            {
                if (res.EndReason != null)
                {
                    reason = res.EndReason;
                }
                if (res.TokenUsage != null)
                {
                    usage = res.TokenUsage;
                }
                if (res.Data != null)
                {
                    if (res.Data is TextContent text)
                    {
                        var last = stack.LastOrDefault();
                        var value = text.Value;

                        if (last is TextContent lastText)
                        {
                            lastText.Value ??= string.Empty;
                            lastText.Value += value;
                        }
                        else
                        {
                            stack.Add(text);
                            last = stack.Last();
                        }

                        yield return new TextContent
                        {
                            Index = message.Content.Count + last.Index,
                            Value = value,
                        };
                    }
                    else if (res.Data is ToolContent tool)
                    {
                        var index = stack.ElementAtOrDefault(tool.Index ?? 0);

                        if (index is ToolContent indexTool)
                        {
                            indexTool.Arguments ??= string.Empty;
                            indexTool.Arguments += tool.Arguments;
                        }
                        else
                        {
                            stack.Add(tool);
                            yield return new ToolContent
                            {
                                Index = message.Content.Count + tool.Index,
                                Id = tool.Id,
                                Name = tool.Name,
                                Arguments = tool.Arguments
                            };
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected content type.");
                    }
                }
            }

            // 메시지 정리 및 재시도
            if (reason == EndReason.MaxTokens)
            {
                session.LastTruncatedIndex = messages.Count - 1;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
                continue;
            }

            // 토큰 사용량 업데이트
            if (usage != null)
            {
                session.TotalTokens = usage.TotalTokens;
            }

            // 메시지 컨텐츠 추가
            message.Content.AddRange(stack);
            
            // 메시지 추가
            if (messages.LastOrDefault()?.Role != MessageRole.Assistant)
            {
                messages.Add(message);
            }

            // 도구 호출
            if (reason == EndReason.ToolCall)
            {
                var toolGroup = message.Content.OfType<ToolContent>();
                foreach (var content in toolGroup)
                {
                    if (content.Name == null || options.Tools == null || content.Result != null)
                        continue;

                    if (options.Tools.TryGetValue(content.Name, out var tool))
                    {
                        content.Status = ToolStatus.Running;
                        yield return content;

                        var result = await tool.InvokeAsync(content.Arguments);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);
                        yield return !result.IsSuccess
                            ? content.FailedError(data)
                            : data.Length > 20_000
                            ? content.FailedLargeResult()
                            : content.Completed(data);
                    }
                    else
                    {
                        yield return content.FailedNotFound();
                    }
                }

                if (toolGroup.Any(x => x.Status == ToolStatus.Failed))
                {
                    failedCount++;
                }
            }
            // 종료
            else
            {
                break;
            }
        }
    }
}
