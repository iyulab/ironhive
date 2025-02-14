using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using Raggle.Driver.Anthropic.ChatCompletion;
using Raggle.Driver.Anthropic.Configurations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MessageRole = Raggle.Abstractions.Messages.MessageRole;

namespace Raggle.Driver.Anthropic;

public class AnthropicChatCompletionService : IChatCompletionService
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionService(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionService(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return models.Select(m => new ChatCompletionModel
        {
            Model = m.ID,
            Owner = "Anthropic",
            CreatedAt = m.CreatedAt,
            ModifiedAt = null,
        });
    }

    /// <inheritdoc />
    public async Task<MessageResponse> ChatCompletionAsync(
        MessageContext context, 
        CancellationToken cancellationToken = default)
    {
        var request = ConvertToRequest(context);
        var response = await _client.PostMessagesAsync(request, cancellationToken);

        var content = context.Messages.Last().Role == MessageRole.Assistant
            ? context.Messages.Last().Content
            : new MessageContentCollection();

        if (response.StopReason == StopReason.ToolUse)
        {
            foreach(var item in response.Content)
            {
                if (item is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else if (item is ToolUseMessageContent toolUse)
                {
                    var id = toolUse.ID;
                    var name = toolUse.Name;
                    var args = new ToolArguments(JsonSerializer.Serialize(toolUse.Input));
                    var result = string.IsNullOrWhiteSpace(name)
                        ? ToolResult.Failed("Tool name is missing")
                        : context.Tools != null && context.Tools.TryGetValue(name, out var tool)
                        ? await tool.InvokeAsync(args)
                        : ToolResult.Failed($"Tool [{name}] not found");

                    content.AddTool(id, name, args, result);
                }
            }

            context.Messages.AddAssistantMessage(content);
            return await ChatCompletionAsync(context, cancellationToken);
        }
        else
        {
            foreach(var item in response.Content)
            {
                if (item is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else
                {
                    Debug.WriteLine($"Unexpected message content type: {item.GetType()}");
                    //throw new InvalidOperationException("Unexpected message content type");
                }
            }

            var result = new MessageResponse
            {
                EndReason = response.StopReason switch
                {
                    StopReason.EndTurn => MessageEndReason.EndTurn,
                    StopReason.MaxTokens => MessageEndReason.MaxTokens,
                    StopReason.StopSequence => MessageEndReason.StopSequence,
                    _ => null
                },
                Model = response.Model,
                Message = new Abstractions.Messages.Message
                {
                    Role = MessageRole.Assistant,
                    Content = content,
                    TimeStamp = DateTime.UtcNow
                },
                TokenUsage = new Abstractions.AI.TokenUsage
                {
                    TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens,
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens
                },
            };

            return result;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> StreamingChatCompletionAsync(
        MessageContext context, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = ConvertToRequest(context);

        var content = new MessageContentCollection();
        await foreach (var response in _client.PostStreamingMessagesAsync(request, cancellationToken))
        {
            // 임시 출력; 추후 삭제
            Console.WriteLine(JsonSerializer.Serialize(response));

            if (response is MessageStartEvent)
            {
                // 시작 이벤트
            }
            else if (response is ContentStartEvent cse)
            {
                // 컨텐츠 블록 생성 시작 이벤트

                if (cse.ContentBlock is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    content.AddTool(tool.ID, tool.Name, null, null);
                }
            }
            else if (response is ContentDeltaEvent cde)
            {
                // 컨텐츠 블록 생성 진행 이벤트

                
                if (cde.ContentBlock is TextDeltaMessageContent text)
                {
                    var item = content.ElementAt(cde.Index) as TextContent
                        ?? throw new InvalidOperationException("Unexpected content type");
                    item.Text += text.Text;
                    yield return new StreamingMessageResponse
                    {
                        Content = new TextContent
                        {
                            Index = item.Index,
                            Text = text.Text
                        }
                    };
                }
                else if (cde.ContentBlock is ToolUseDeltaMessageContent tool)
                {
                    var item = content.ElementAt(cde.Index) as ToolContent
                        ?? throw new InvalidOperationException("Unexpected content type");
                    item.Arguments ??= new ToolArguments();
                    item.Arguments.Append(tool.PartialJson);

                    yield return new StreamingMessageResponse { Content = item };
                }
            }
            else if (response is ContentStopEvent)
            {
                // 컨텐츠 블록 생성 종료 이벤트

                // nothing to do
            }
            else if (response is MessageDeltaEvent messageDelta)
            {
                // 생성 종료 이벤트

                var reason = messageDelta.Delta?.StopReason;
                if (reason == StopReason.ToolUse)
                {
                    foreach (var item in content)
                    {
                        if (item is ToolContent toolContent)
                        {
                            toolContent.Result = string.IsNullOrWhiteSpace(toolContent.Name)
                                    ? ToolResult.Failed("Tool name is missing")
                                    : context.Tools != null && context.Tools.TryGetValue(toolContent.Name, out var tool)
                                    ? await tool.InvokeAsync(toolContent.Arguments)
                                    : ToolResult.Failed($"Tool [{toolContent.Name}] not found");
                            yield return new StreamingMessageResponse
                            {
                                Content = toolContent
                            };
                        }
                    }

                    context.Messages.AddAssistantMessage(content);
                    await foreach (var stream in StreamingChatCompletionAsync(context, cancellationToken))
                    {
                        yield return stream;
                    }
                }
                else
                {
                    yield return new StreamingMessageResponse
                    {
                        Model = context.Model,
                        EndReason = reason switch
                        {
                            StopReason.EndTurn => MessageEndReason.EndTurn,
                            StopReason.StopSequence => MessageEndReason.StopSequence,
                            StopReason.MaxTokens => MessageEndReason.MaxTokens,
                            _ => null
                        },
                        TokenUsage = new Abstractions.AI.TokenUsage
                        {
                            TotalTokens = messageDelta.Usage?.InputTokens + messageDelta.Usage?.OutputTokens,
                            InputTokens = messageDelta.Usage?.InputTokens,
                            OutputTokens = messageDelta.Usage?.OutputTokens
                        }
                    };
                }
            }
            else if (response is MessageStopEvent)
            {
                // 종료 이벤트
                // nothing to do
            }
            else if (response is PingEvent)
            {
                // nothing to do
            }
            else if (response is ErrorEvent error)
            {
                throw new InvalidOperationException(error.Error.ToString());
            }
            else
            {
                // unexpected event nothing to do
                Debug.WriteLine($"Unexpected event: {response.GetType()}");
            }
        }
    }

    private MessagesRequest ConvertToRequest(MessageContext context)
    {
        var request = new MessagesRequest
        {
            Model = context.Model,
            System = context.MessagesOptions?.System,
            Messages = context.Messages.ToAnthropic().ToArray(),
            MaxTokens = context.Parameters?.MaxTokens,
            Temperature = context.Parameters?.Temperature,
            TopK = context.Parameters?.TopK,
            TopP = context.Parameters?.TopP,
            StopSequences = context.Parameters?.StopSequences?.ToArray(),
        };

        if (context.Tools != null && context.Tools.Count > 0)
        {
            request.Tools = context.Tools.Select(t => new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = new
                {
                    Properties = t.Properties,
                    Required = t.Required,
                }
            }).ToArray();
        }

        return request;
    }
}
