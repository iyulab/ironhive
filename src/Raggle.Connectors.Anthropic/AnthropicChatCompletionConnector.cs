using Raggle.Abstractions.AI;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.ChatCompletion.Tools;
using Raggle.Connectors.Anthropic.ChatCompletion;
using Raggle.Connectors.Anthropic.Configurations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TokenUsage = Raggle.Abstractions.ChatCompletion.TokenUsage;
using ManualToolChoice = Raggle.Abstractions.ChatCompletion.Tools.ManualToolChoice;
using AnthropicManualToolChoice = Raggle.Connectors.Anthropic.ChatCompletion.ManualToolChoice;
using AutoToolChoice = Raggle.Abstractions.ChatCompletion.Tools.AutoToolChoice;
using AnthropicAutoToolChoice = Raggle.Connectors.Anthropic.ChatCompletion.AutoToolChoice;

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
    public async Task<ChatCompletionResult<IMessage>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);
        var res = await _client.PostMessagesAsync(req, cancellationToken);

        var content = new MessageContentCollection();
        foreach (var item in res.Content)
        {
            if (item is TextMessageContent text)
            {
                content.AddText(text.Text);
            }
            else if (item is ToolUseMessageContent tool)
            {
                content.AddTool(tool.ID, tool.Name, new ToolArguments(tool.Input), null);
            }
            else
            {
                Debug.WriteLine($"Unexpected message content type: {item.GetType()}");
            }
        }

        var result = new ChatCompletionResult<IMessage>
        {
            EndReason = res.StopReason switch
            {
                StopReason.ToolUse => EndReason.ToolCall,
                StopReason.EndTurn => EndReason.EndTurn,
                StopReason.MaxTokens => EndReason.MaxTokens,
                StopReason.StopSequence => EndReason.StopSequence,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                TotalTokens = res.Usage.InputTokens + res.Usage.OutputTokens,
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            },
            Data = new AssistantMessage
            {
                Content = content
            }
        };

        return result;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(messages, options);

        await foreach (var res in _client.PostStreamingMessagesAsync(req, cancellationToken))
        {
            if (res is MessageStartEvent)
            {
                // 시작 이벤트
            }
            else if (res is ContentStartEvent cse)
            {
                // 컨텐츠 블록 생성 시작 이벤트

                if (cse.ContentBlock is TextMessageContent text)
                {
                    yield return new ChatCompletionResult<IMessageContent>
                    {
                        Data = new TextContent
                        {
                            Index = cse.Index,
                            Text = text.Text
                        }
                    };
                }
                else if (cse.ContentBlock is ToolUseMessageContent tool)
                {
                    yield return new ChatCompletionResult<IMessageContent>
                    {
                        Data = new ToolContent
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
            else if (res is ContentDeltaEvent cde)
            {
                // 컨텐츠 블록 생성 진행 이벤트
                if (cde.ContentBlock is TextDeltaMessageContent text)
                {
                    yield return new ChatCompletionResult<IMessageContent>
                    {
                        Data = new TextContent
                        {
                            Index = cde.Index,
                            Text = text.Text
                        }
                    };
                }
                else if (cde.ContentBlock is ToolUseDeltaMessageContent tool)
                {
                    yield return new ChatCompletionResult<IMessageContent>
                    {
                        Data = new ToolContent
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
            else if (res is ContentStopEvent)
            {
                // 컨텐츠 블록 생성 종료 이벤트
            }
            else if (res is MessageDeltaEvent mde)
            {
                // 생성 종료 이벤트
                yield return new ChatCompletionResult<IMessageContent>
                {
                    EndReason = mde.Delta?.StopReason switch
                    {
                        StopReason.ToolUse => EndReason.ToolCall,
                        StopReason.EndTurn => EndReason.EndTurn,
                        StopReason.StopSequence => EndReason.StopSequence,
                        StopReason.MaxTokens => EndReason.MaxTokens,
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
            else if (res is MessageStopEvent)
            {
                // 종료 이벤트
            }
            else if (res is PingEvent)
            {
                // ping 이벤트
            }
            else if (res is ErrorEvent error)
            {
                throw new InvalidOperationException(error.Error.ToString());
            }
            else
            {
                Debug.WriteLine($"Unexpected event: {res.GetType()}");
            }
        }
    }

    private static MessagesRequest BuildRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        var request = new MessagesRequest
        {
            Model = options.Model,
            System = options.System,
            Messages = messages.ToAnthropic(),
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            TopK = options.TopK,
            TopP = options.TopP,
            StopSequences = options.StopSequences,
        };

        if (options.Tools != null && options.Tools.Count > 0)
        {
            request.Tools = options.Tools.Select(t => new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = new
                {
                    Properties = t.Parameters,
                    Required = t.Required,
                }
            });

            if (options.ToolChoice != null)
            {
                if (options.ToolChoice is ManualToolChoice manual)
                {
                    request.ToolChoice = new AnthropicManualToolChoice
                    {
                        Name = manual.ToolName,
                    };
                }
                else if (options.ToolChoice is AutoToolChoice)
                {
                    request.ToolChoice = new AnthropicAutoToolChoice();
                }
            }
        }

        return request;
    }
}
