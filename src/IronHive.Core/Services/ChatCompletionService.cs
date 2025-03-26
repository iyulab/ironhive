using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Core.Services;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IHiveServiceStore _store;
    private readonly IToolManager _manager;

    public ChatCompletionService(IHiveServiceStore store, IToolManager manager)
    {
        _store = store;
        _manager = manager;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();
        var connectors = _store.GetServices<IChatCompletionConnector>();

        foreach (var (key, conn) in connectors)
        {
            var providerModels = await conn.GetModelsAsync(cancellationToken);
            models.AddRange(providerModels.Select(m =>
            {
                m.Provider = key;
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
        if (!_store.TryGetService<IChatCompletionConnector>(provider, out var conn))
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
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        var model = await GetModelAsync(options.Provider, options.Model, cancellationToken);
        if(model.Capabilities.SupportsVision == false)
        {
            var users = messages.Where(x => x.Role == MessageRole.User).ToList();
            var anyImg = users.Any(x => x.Content.Any(c => c is ImageContent));
            if (anyImg)
            {
                throw new NotSupportedException("Vision is not supported by the model.");
            }
        }

        if (model.Capabilities.SupportsToolCall == false)
        {
            var anyTools = options.Tools?.Any() ?? false;
            if (anyTools)
            {
                throw new NotSupportedException("Tool call is not supported by the model.");
            }
        }

        if (model.Capabilities.SupportsAudio == true)
        {
            // do nothing
        }

        if (model.Capabilities.SupportsReasoning == true)
        {
            // do nothing
        }

        int failedCount = 0;
        var message = new Message(MessageRole.Assistant);
        var request = await CreateChatCompletionRequest(messages, options);

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
                            content.Failed(data);
                        else if (data.Length > 20_000)
                            content.TooMuchResult();
                        else
                            content.Completed(data);
                    }
                    else
                    {
                        content.NotFoundTool();
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
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        //if(!string.IsNullOrEmpty(session.Summary))
        //{
        //    options.System = $"\n### Conversation Summary\n{session.Summary}\n";
        //}

        int failedCount = 0;
        var message = new Message(MessageRole.Assistant);
        var request = await CreateChatCompletionRequest(messages, options);

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
                            ? content.Failed(data)
                            : data.Length > 20_000
                            ? content.TooMuchResult()
                            : content.Completed(data);
                    }
                    else
                    {
                        yield return content.NotFoundTool();
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

    private async Task<ChatCompletionRequest> CreateChatCompletionRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        var instructions = options.Instructions;
        var tools = new ToolCollection();
        if (options.Tools != null)
        {
            foreach (var (key, value) in options.Tools)
            {
                var service = _manager.GetToolService(key);
                await service.HandleInitializedAsync(value);
                instructions += await service.HandleSetInstructionsAsync(value);
                var coll = _manager.CreateFromObject(service);
                tools.AddRange(coll);
            }
        }

        var request = new ChatCompletionRequest
        {
            Model = options.Model,
            System = instructions,
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
