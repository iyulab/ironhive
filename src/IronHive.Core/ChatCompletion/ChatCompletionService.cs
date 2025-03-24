using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly IReadOnlyDictionary<string, IToolService> _tools;

    public ChatCompletionService(
        IReadOnlyDictionary<string, IChatCompletionConnector> connectors,
        IReadOnlyDictionary<string, IToolService> tools)
    {
        _connectors = connectors;
        _tools = tools;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();

        foreach (var kvp in _connectors)
        {
            var providerModels = await kvp.Value.GetModelsAsync(cancellationToken);
            models.AddRange(providerModels.Select(m =>
            {
                m.Provider = kvp.Key;
                return m;
            }));
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionModel> GetModelAsync(
        string provider,
        string model,
        CancellationToken cancellationToken = default)
    {
        if (!_connectors.TryGetValue(provider, out var conn))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var providerModel = await conn.GetModelAsync(provider, cancellationToken);
        providerModel.Provider = provider;
        return providerModel;
    }

    /// <inheritdoc />
    public async Task<Message> ExecuteAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!_connectors.TryGetValue(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        //if (!string.IsNullOrEmpty(session.Summary))
        //{
        //    options.System = $"\n### Conversation Summary\n{session.Summary}\n";
        //}

        int failedCount = 0;
        var message = new Message(MessageRole.Assistant);
        var request = PrepareRequest(messages, options);

        while (failedCount < options.MaxToolAttempts)
        {
            var res = await conn.GenerateMessageAsync(request, cancellationToken);

            // 메시지 정리 및 재시도
            if (res.EndReason == EndReason.MaxTokens)
            {
                //session.LastTruncatedIndex = request.Messages.Count - 1;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
                continue;
            }

            // 토큰 사용량 업데이트
            if (res.TokenUsage != null)
            {
                //session.TotalTokens = res.TokenUsage.TotalTokens;
            }

            // 메시지 컨텐츠 추가
            message.Content.AddRange(res.Data?.Content ?? []);

            // 메시지 추가
            if (request.Messages.LastOrDefault()?.Role != MessageRole.Assistant)
            {
                request.Messages.Add(message);
            }

            // 도구 호출
            if (res.EndReason == EndReason.ToolCall)
            {
                var toolGroup = message.Content.OfType<ToolContent>() ?? [];
                foreach (var content in toolGroup)
                {
                    if (request.Tools == null || string.IsNullOrEmpty(content.Name))
                        continue;
                    if (request.Tools.TryGetValue(content.Name, out var tool))
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
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_connectors.TryGetValue(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        //if(!string.IsNullOrEmpty(session.Summary))
        //{
        //    options.System = $"\n### Conversation Summary\n{session.Summary}\n";
        //}

        int failedCount = 0;
        var message = new Message(MessageRole.Assistant);
        var request = PrepareRequest(messages, options);

        while (failedCount < options.MaxToolAttempts)
        {
            var stack = new MessageContentCollection();
            EndReason? reason = null;
            TokenUsage? usage = null;
            
            // 메시지 생성
            await foreach (var res in conn.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                // 종료 이유
                if (res.EndReason != null)
                {
                    reason = res.EndReason;
                }

                // 토큰 사용량
                if (res.TokenUsage != null)
                {
                    usage = res.TokenUsage;
                }

                // 메시지 컨텐츠 추가
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
                //session.LastTruncatedIndex = request.Messages.Count - 1;
                throw new NotImplementedException("Max tokens reached. not supported yet.");
                continue;
            }

            // 토큰 사용량 업데이트
            if (usage != null)
            {
                //session.TotalTokens = usage.TotalTokens;
            }

            // 메시지 컨텐츠 추가
            message.Content.AddRange(stack);
            
            // 어시스턴트 메시지 추가
            if (request.Messages.LastOrDefault()?.Role != MessageRole.Assistant)
            {
                request.Messages.Add(message);
            }

            // 도구 호출
            if (reason == EndReason.ToolCall)
            {
                var toolGroup = message.Content.OfType<ToolContent>();
                foreach (var content in toolGroup)
                {
                    if (content.Name == null || request.Tools == null || content.Result != null)
                        continue;

                    if (request.Tools.TryGetValue(content.Name, out var tool))
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

    private ChatCompletionRequest PrepareRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        var tools = new FunctionToolCollection();
        if (options.Tools != null)
        {
            foreach (var (key, option) in options.Tools)
            {
                if (_tools.TryGetValue(key, out var instance))
                {
                    instance.InitializeToolExecutionAsync(option);
                    var coll = FunctionToolFactory.CreateFromObject(instance);
                    tools.AddRange(coll);
                }
            }
        }

        var request = new ChatCompletionRequest
        {
            Model = options.Model,
            System = options.Instructions,
            Tools = tools,
            Messages = messages.Clone(), // 메시지 복사
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            TopK = options.TopK,
            TopP = options.TopP,
            StopSequences = options.StopSequences
        };

        return request;
    }
}
