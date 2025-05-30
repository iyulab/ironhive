using System.Runtime.CompilerServices;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Providers.OpenAI.ChatCompletion;
using MessageGenerationRequest = IronHive.Abstractions.Message.MessageGenerationRequest;
using TextMessageContent = IronHive.Abstractions.Message.Content.TextMessageContent;
using AssistantMessage = IronHive.Abstractions.Message.Roles.AssistantMessage;
using OpenAIAssistantMessage = IronHive.Providers.OpenAI.ChatCompletion.AssistantMessage;
using MessageContent = IronHive.Abstractions.Message.MessageContent;
using OpenAIMessageContent = IronHive.Providers.OpenAI.ChatCompletion.MessageContent;

namespace IronHive.Providers.OpenAI;

public class OpenAIMessageGenerationProvider : IMessageGenerationProvider
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIMessageGenerationProvider(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIMessageGenerationProvider(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = request.ToOpenAI();
        var res = await _client.PostChatCompletionAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var content = new List<MessageContent>();

        // 텍스트 생성
        var text = choice?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(text))
        {
            content.Add(new TextMessageContent
            {
                Value = text
            });
        }

        // 툴 호출
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            foreach (var tool in tools.OrderBy(t => t.Index))
            {
                content.Add(new ToolMessageContent
                {
                    Id = tool.Id,
                    Name = tool.Function?.Name,
                    Input = tool.Function?.Arguments,
                });
            }
        }

        return new MessageResponse
        {
            DoneReason = choice?.FinishReason switch
            {
                FinishReason.ToolCalls => MessageDoneReason.ToolCall,
                FinishReason.Stop => MessageDoneReason.EndTurn,
                FinishReason.Length => MessageDoneReason.MaxTokens,
                FinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                _ => null
            },
            Message = new AssistantMessage
            {
                Id = res.Id ?? Guid.NewGuid().ToString(),
                Name = null,
                Model = res.Model,
                Content = content,
                Timestamp = DateTime.UtcNow,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage?.PromptTokens,
                OutputTokens = res.Usage?.CompletionTokens
            }
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToOpenAI();

        OpenAIAssistantMessage? message = null;
        var id = string.Empty;
        var model = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        await foreach (var res in _client.PostStreamingChatCompletionAsync(req, cancellationToken))
        {
            // 메시지 시작
            if (message == null)
            {
                message = new OpenAIAssistantMessage();
                id = res.Id ?? Guid.NewGuid().ToString();
                model = res.Model;
                yield return new StreamingMessageBeginResponse
                {
                    Id = id
                };
            }

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.Usage != null)
            {
                usage = new MessageTokenUsage
                {
                    InputTokens = res.Usage.PromptTokens,
                    OutputTokens = res.Usage.CompletionTokens
                };
            }

            // 메시지 확인 및 건너뛰기
            var choice = res.Choices?.FirstOrDefault();
            if (choice == null)
                continue;

            // 종료 메시지
            if (choice.FinishReason != null)
            {
                reason = choice.FinishReason switch
                {
                    FinishReason.ToolCalls => MessageDoneReason.ToolCall,
                    FinishReason.Stop => MessageDoneReason.EndTurn,
                    FinishReason.Length => MessageDoneReason.MaxTokens,
                    FinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                    _ => null
                };
            }

            // 메시지 확인 및 건너뛰기
            var delta = choice.Delta;
            if (delta == null)
                continue;

            // 툴 사용
            var tools = delta.ToolCalls;
            if (tools != null)
            {
                for (var i = 0; i < tools.Count; i++)
                {
                    var tool = tools.ElementAt(i);
                    var toolIndex = tool.Index ?? i;

                    // 텍스트 컨텐츠가 있을경우 인덱스를 +1
                    var outIndex = message.Content == null ? toolIndex : toolIndex + 1;
                    var existing = message.ToolCalls?.ElementAtOrDefault(toolIndex);

                    // 새로운 툴을 생성
                    if (existing == null)
                    {
                        message.ToolCalls ??= [];
                        message.ToolCalls.Add(new ToolCall
                        {
                            Index = toolIndex,
                            Id = tool.Id,
                            Function = new FunctionCall
                            {
                                Name = tool.Function?.Name,
                                Arguments = tool.Function?.Arguments
                            }
                        });
                        yield return new StreamingContentAddedResponse
                        {
                            Index = outIndex,
                            Content = new ToolMessageContent
                            {
                                Id = tool.Id,
                                Name = tool.Function?.Name,
                                Input = tool.Function?.Arguments
                            }
                        };
                    }
                    // 생성한 툴의 입력값을 생성
                    else
                    {
                        yield return new StreamingContentDeltaResponse
                        {
                            Index = outIndex,
                            Delta = new ToolDeltaContent
                            {
                                Input = tool.Function?.Arguments ?? string.Empty,
                            }
                        };
                    }
                }
            }

            // 텍스트 생성
            var text = delta.Content;
            if (text != null)
            {
                // 새로운 텍스트를 시작
                if (message.Content == null)
                {
                    message.Content = text;
                    yield return new StreamingContentAddedResponse
                    {
                        Index = 0,
                        Content = new TextMessageContent
                        {
                            Value = text
                        }
                    };
                }
                // 텍스트 메시지 업데이트
                else
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = 0,
                        Delta = new TextDeltaContent
                        {
                            Text = text
                        }
                    };
                }
            }
            
        }

        // 종료
        yield return new StreamingMessageDoneResponse
        {
            Id = id,
            Name = null,
            DoneReason = reason,
            TokenUsage = usage,
            Timestamp = DateTime.UtcNow,
        };
    }
}
