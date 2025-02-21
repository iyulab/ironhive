using Raggle.Abstractions.AI;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Connectors.Anthropic.ChatCompletion;
using Raggle.Connectors.Anthropic.Configurations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TokenUsage = Raggle.Abstractions.ChatCompletion.TokenUsage;

namespace Raggle.Connectors.Anthropic;

public class AnthropicChatCompletionConnector : IChatCompletionConnector
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionConnector(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionConnector(string apiKey)
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
    public async Task<ChatCompletionResponse<IMessage>> GenerateMessageAsync(
        ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToAnthropic(request);
        var response = await _client.PostMessagesAsync(_request, cancellationToken);

        var content = new MessageContentCollection();
        foreach (var item in response.Content)
        {
            if (item is TextMessageContent text)
            {
                content.AddText(text.Text);
            }
            else if (item is ToolUseMessageContent tool)
            {
                content.AddTool(tool.ID, tool.Name, null, null);
            }
            else
            {
                Debug.WriteLine($"Unexpected message content type: {item.GetType()}");
            }
        }

        var result = new ChatCompletionResponse<IMessage>
        {
            EndReason = response.StopReason switch
            {
                StopReason.ToolUse => ChatCompletionEndReason.ToolCall,
                StopReason.EndTurn => ChatCompletionEndReason.EndTurn,
                StopReason.MaxTokens => ChatCompletionEndReason.MaxTokens,
                StopReason.StopSequence => ChatCompletionEndReason.StopSequence,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens,
                InputTokens = response.Usage.InputTokens,
                OutputTokens = response.Usage.OutputTokens
            },
            Content = new AssistantMessage
            {
                Content = content
            }
        };

        return result;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = ConvertToAnthropic(request);

        await foreach (var response in _client.PostStreamingMessagesAsync(_request, cancellationToken))
        {
            if (response is MessageStartEvent)
            {
                // 시작 이벤트
            }
            else if (response is ContentStartEvent cse)
            {
                // 컨텐츠 블록 생성 시작 이벤트

                if (cse.ContentBlock is TextMessageContent text)
                {
                    yield return new ChatCompletionResponse<IMessageContent>
                    {
                        Content = new TextContent
                        {
                            Index = cse.Index,
                            Text = text.Text
                        }
                    };
                }
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    yield return new ChatCompletionResponse<IMessageContent>
                    {
                        Content = new ToolContent
                        {
                            Index = cse.Index,
                            Id = tool.ID,
                            Name = tool.Name,
                            Arguments = null,
                            Result = null
                        }
                    };
                }
            }
            else if (response is ContentDeltaEvent cde)
            {
                // 컨텐츠 블록 생성 진행 이벤트
                if (cde.ContentBlock is TextDeltaMessageContent text)
                {
                    yield return new ChatCompletionResponse<IMessageContent>
                    {
                        Content = new TextContent
                        {
                            Index = cde.Index,
                            Text = text.Text
                        }
                    };
                }
                else if (cde.ContentBlock is ToolUseDeltaMessageContent tool)
                {
                    yield return new ChatCompletionResponse<IMessageContent>
                    {
                        Content = new ToolContent
                        {
                            Index = cde.Index,
                            Id = null,
                            Name = null,
                            Arguments = null,
                            Result = null
                        }
                    };
                }
            }
            else if (response is ContentStopEvent)
            {
                // 컨텐츠 블록 생성 종료 이벤트
            }
            else if (response is MessageDeltaEvent mde)
            {
                // 생성 종료 이벤트
                yield return new ChatCompletionResponse<IMessageContent>
                {
                    EndReason = mde.Delta?.StopReason switch
                    {
                        StopReason.ToolUse => ChatCompletionEndReason.ToolCall,
                        StopReason.EndTurn => ChatCompletionEndReason.EndTurn,
                        StopReason.StopSequence => ChatCompletionEndReason.StopSequence,
                        StopReason.MaxTokens => ChatCompletionEndReason.MaxTokens,
                        _ => null
                    },
                    TokenUsage = new TokenUsage
                    {
                        TotalTokens = mde.Usage?.InputTokens + mde.Usage?.OutputTokens,
                        InputTokens = mde.Usage?.InputTokens,
                        OutputTokens = mde.Usage?.OutputTokens
                    },
                };
            }
            else if (response is MessageStopEvent)
            {
                // 종료 이벤트
            }
            else if (response is PingEvent)
            {
                // ping 이벤트
            }
            else if (response is ErrorEvent error)
            {
                throw new InvalidOperationException(error.Error.ToString());
            }
            else
            {
                Debug.WriteLine($"Unexpected event: {response.GetType()}");
            }
        }
    }

    private static MessagesRequest ConvertToAnthropic(ChatCompletionRequest request)
    {
        var _request = new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = request.Messages.ToAnthropic(),
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopK = request.TopK,
            TopP = request.TopP,
            StopSequences = request.StopSequences,
        };

        if (request.Tools != null && request.Tools.Count > 0)
        {
            _request.Tools = request.Tools.Select(t => new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = new
                {
                    Properties = t.Parameters,
                    Required = t.Required,
                }
            });

            if (request.ToolChoice != null)
            {
                if (request.ToolChoice is ManualToolChoice manual)
                {
                    _request.ToolChoice = new ManualToolChoice
                    {
                        Name = manual.Name,
                    };
                }
                else if (request.ToolChoice is AutoToolChoice)
                {
                    _request.ToolChoice = new AutoToolChoice();
                }
            }
        }

        return _request;
    }
}
