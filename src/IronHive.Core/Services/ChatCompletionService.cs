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
    private readonly IToolHandlerManager _manager;

    public ChatCompletionService(IHiveServiceStore store, IToolHandlerManager manager)
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
        var conn = _store.GetService<IChatCompletionConnector>(provider);
        var providerModel = await conn.GetModelAsync(provider, cancellationToken);
        providerModel.Provider = provider;
        return providerModel;
    }

    /// <inheritdoc />
    public Task<AssistantMessage> GenerateMessageAsync(
        UserMessage message, 
        ChatCompletionOptions options, 
        CancellationToken cancellationToken = default)
    {
        var messages = new MessageCollection { message };
        return GenerateMessageAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IAssistantContent> GenerateStreamingMessageAsync(
        UserMessage message, 
        ChatCompletionOptions options, 
        CancellationToken cancellationToken = default)
    {
        var messages = new MessageCollection { message };
        return GenerateStreamingMessageAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AssistantMessage> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        var message = new AssistantMessage();
        var request = await CreateChatCompletionRequest(messages, options);

        while (!cancellationToken.IsCancellationRequested)
        {
            var res = await conn.GenerateMessageAsync(request, cancellationToken);

            // 메시지 컨텐츠 추가
            if (res.Data?.Content != null)
                message.Content.AddRange(res.Data.Content);

            // 메시지 추가
            if (request.Messages.LastOrDefault() is not AssistantMessage)
                request.Messages.Add(message);

            // 토큰 사용량 업데이트
            if (res.TokenUsage != null)
            { }

            // 도구 호출
            if (res.EndReason == EndReason.ToolCall)
            {
                var toolContentGroup = message.Content.OfType<AssistantToolContent>() ?? [];
                foreach (var content in toolContentGroup)
                {
                    if (string.IsNullOrEmpty(content.Name) || request.Tools == null || content.Result != null)
                    {
                        continue;
                    }

                    if (request.Tools.TryGetValue(content.Name, out var tool))
                    {
                        var result = await tool.InvokeAsync(content.Arguments, cancellationToken);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                        if (!result.IsSuccess)
                            content.Failed(data);
                        else if (data.Length > 30_000)
                            content.TooMuchResult();
                        else
                            content.Completed(data);
                    }
                    else
                    {
                        content.NotFoundTool();
                    }
                }
            }
            else if (res.EndReason == EndReason.MaxTokens)
            {
                break;
            }
            else if (res.EndReason == EndReason.ContentFilter)
            {
                break;
            }
            else if (res.EndReason == EndReason.StopSequence)
            {
                break;
            }
            else
            {
                break;
            }
        }

        return message;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IAssistantContent> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IChatCompletionConnector>(options.Provider, out var conn))
            throw new KeyNotFoundException($"Service key '{options.Provider}' not found.");

        var message = new AssistantMessage();
        var request = await CreateChatCompletionRequest(messages, options);

        while (!cancellationToken.IsCancellationRequested)
        {
            EndReason? reason = null;
            TokenUsage? usage = null;
            var stack = new AssistantContentCollection();

            // 메시지 생성
            await foreach (var res in conn.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                // 생성 종료 이유
                if (res.EndReason != null)
                    reason = res.EndReason;

                // 토큰 사용량 업데이트
                if (res.TokenUsage != null)
                    usage = res.TokenUsage;

                // 컨텐츠 추가
                if (res.Data != null)
                {
                    if (res.Data is AssistantTextContent text)
                    {
                        var last = stack.LastOrDefault();
                        var value = text.Value;

                        if (last is AssistantTextContent lastText)
                        {
                            lastText.Value ??= string.Empty;
                            lastText.Value += value;
                        }
                        else
                        {
                            stack.Add(text);
                            last = stack.Last();
                        }

                        yield return new AssistantTextContent
                        {
                            Index = message.Content.Count + last.Index, // 실제 인덱스 계산
                            Value = value,
                        };
                    }
                    else if (res.Data is AssistantToolContent tool)
                    {
                        var exist = stack.ElementAtOrDefault(tool.Index ?? 0);

                        if (exist is AssistantToolContent existTool)
                        {
                            existTool.Arguments ??= string.Empty;
                            existTool.Arguments += tool.Arguments;
                        }
                        else
                        {
                            stack.Add(tool);
                            yield return new AssistantToolContent
                            {
                                Status = ToolStatus.Pending,
                                Index = message.Content.Count + tool.Index, // 실제 인덱스 계산
                                Id = tool.Id,
                                Name = tool.Name,
                            };
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected content type.");
                    }
                }
            }

            // 어시스턴트 메시지에 컨텐츠를 추가
            if (stack.Count > 0)
                message.Content.AddRange(stack);

            // 어시스턴트 메시지 추가 (마지막 메시지가 아닌 경우)
            if (request.Messages.LastOrDefault() is not AssistantMessage)
                request.Messages.Add(message);

            // 토큰 사용량 업데이트
            if (usage != null)
            { }

            // 도구 호출
            if (reason == EndReason.ToolCall)
            {
                var toolContentGroup = message.Content.OfType<AssistantToolContent>();
                foreach (var content in toolContentGroup)
                {
                    if (string.IsNullOrEmpty(content.Name) || request.Tools == null || content.Result != null)
                    {
                        continue;
                    }

                    if (request.Tools.TryGetValue(content.Name, out var tool))
                    {
                        content.Status = ToolStatus.Running;
                        yield return content;

                        var result = await tool.InvokeAsync(content.Arguments, cancellationToken);
                        var data = JsonSerializer.Serialize(result.Data, JsonDefaultOptions.Options);

                        if (!result.IsSuccess)
                            content.Failed(data);
                        else if (data.Length > 30_000)
                            content.TooMuchResult();
                        else
                            content.Completed(data);

                        yield return content;
                    }
                    else
                    {
                        content.NotFoundTool();
                        yield return content;
                    }
                }
            }
            else if (reason == EndReason.MaxTokens)
            {
                break;
            }
            else if (reason == EndReason.ContentFilter)
            {
                break;
            }
            else if (reason == EndReason.StopSequence)
            {
                break;
            }
            else
            {
                break;
            }
        }
    }

    // 채팅 완성 요청을 생성합니다.
    private async Task<ChatCompletionRequest> CreateChatCompletionRequest(
        MessageCollection messages, 
        ChatCompletionOptions options)
    {
        var instructions = options.Instructions;
        var tools = new ToolCollection();
        if (options.Tools != null)
        {
            foreach (var (key, value) in options.Tools)
            {
                await _manager.HandleInitializedAsync(key, value);
                instructions += await _manager.HandleSetInstructionsAsync(key, value);
                var coll = _manager.CreateToolCollection(key);
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

    // 모델이 지원하는 기능을 확인합니다.
    //private async Task<bool> CanGenerateMessage(ChatCompletionOptions options)
    //{
    //    var model = await GetModelAsync(options.Provider, options.Model);
    //    if (model.Capabilities.SupportsVision == false)
    //    {
    //        var users = messages.OfType<UserMessage>();
    //        var anyImg = users.Any(x => x.Content.Any(c => c is UserImageContent));
    //        if (anyImg)
    //        {
    //            throw new NotSupportedException("Vision is not supported by the model.");
    //        }
    //    }

    //    if (model.Capabilities.SupportsToolCall == false)
    //    {
    //        var anyTools = options.Tools?.Any() ?? false;
    //        if (anyTools)
    //        {
    //            throw new NotSupportedException("Tool call is not supported by the model.");
    //        }
    //    }

    //    if (model.Capabilities.SupportsAudio == true)
    //    {
    //        // do nothing
    //    }

    //    if (model.Capabilities.SupportsReasoning == true)
    //    {
    //        // do nothing
    //    }
    //    return true;
    //}
}
