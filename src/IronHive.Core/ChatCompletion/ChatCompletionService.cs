using DocumentFormat.OpenXml.Office.CustomUI;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.ChatCompletion.Messages;
using IronHive.Abstractions.ChatCompletion.Tools;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly IModelParser _parser;

    public ChatCompletionService(
        IHiveServiceContainer container,
        IModelParser parser)
    {
        _connectors = container.GetKeyedServices<IChatCompletionConnector>();
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
                        content.Status = result.IsSuccess ? ToolStatus.Completed : ToolStatus.Failed;
                        content.Result = JsonSerializer.Serialize(result.Data);
                    }
                    else
                    {
                        content.Status = ToolStatus.Failed;
                        content.Result = $"Tool '{content.Name}' not found.";
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
        #region Tool Test
        options.Tools ??= new FunctionToolCollection();
        var tt = FunctionToolFactory.CreateFromObject<TestTool>();
        options.Tools.AddRange(tt);
        #endregion

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
                        content.Status = result.IsSuccess ? ToolStatus.Completed : ToolStatus.Failed;
                        content.Result = JsonSerializer.Serialize(result.Data);

                        yield return content;
                    }
                    else
                    {
                        content.Status = ToolStatus.Failed;
                        content.Result = $"Tool '{content.Name}' not found.";

                        yield return content;
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

public class TestTool
{
    [FunctionTool("date-previous")]
    public DateTime GetPreviousTime(int days)
    {
        if (days > 0)
        {
            throw new Exception("Input value is positive. Please provide a negative value to calculate a previous date.");
        }
        else
        {
            return DateTime.UtcNow.AddDays(days);
        }
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