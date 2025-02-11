using Raggle.Driver.OpenAI.ChatCompletion;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Abstractions.Tools;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Raggle.Driver.OpenAI.Extensions;

namespace Raggle.Driver.OpenAI;

public class OpenAIChatCompletionService : IChatCompletionService
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatCompletionService(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIChatCompletionService(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Where(m => m.IsChatCompletion())
                    .Select(m => new ChatCompletionModel
                    {
                        Model = m.ID,
                        Owner = m.OwnedBy,
                        CreatedAt = m.Created,
                    });
    }

    /// <inheritdoc />
    public async Task<MessageResponse> ChatCompletionAsync(
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToRequest(context);
        var response = await _client.PostChatCompletionAsync(_request, cancellationToken);
        var choice = response.Choices?.First();

        var content = context.Messages.Last().Role == MessageRole.Assistant
            ? context.Messages.Last().Content
            : new MessageContentCollection();

        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var index = toolCall.Index ?? 0;
                var id = toolCall.ID;
                var name = toolCall.Function?.Name;
                var args = new FunctionArguments(toolCall.Function?.Arguments ?? string.Empty);

                var result = string.IsNullOrWhiteSpace(name)
                    ? FunctionResult.Failed("Function name is required.")
                    : context.Tools != null && context.Tools.TryGetValue(name, out var function)
                    ? await function.InvokeAsync(args)
                    : FunctionResult.Failed($"Function '{name}' not exist.");

                content.AddTool(id, name, args, result);
            }
            context.Messages.AddAssistantMessage(content);
            return await ChatCompletionAsync(context, cancellationToken);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(choice?.Message?.Content))
            {
                content.AddText(choice.Message.Content);
            }
            else
            {
                Debug.WriteLine("No content found in the response.");
            }

            var result = new MessageResponse
            {
                EndReason = choice?.FinishReason switch
                {
                    FinishReason.Stop => MessageEndReason.EndTurn,
                    FinishReason.Length => MessageEndReason.MaxTokens,
                    FinishReason.ContentFilter => MessageEndReason.ContentFilter,
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
                    TotalTokens = response.Usage?.TotalTokens,
                    InputTokens = response.Usage?.PromptTokens,
                    OutputTokens = response.Usage?.CompletionTokens
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
        var _request = ConvertToRequest(context);
        var toolContents = new Dictionary<int, ToolContent>();

        /*
         * 1. Request 변환 => To OpenAI
         * 2. Assistant Message 생성
         * 3. Foreach Loop Until MaxTry 
         *    3.0 메시지 Concat 또는 Pass
         *    3.1 요청 ToOpenAI
         *    3.2 응답 Message 저장 With Return
         *    3.3 ToolUse With 2번 메시지
         *    3.4 다시 Loop
         *  4. Context??? 필요
         */

        await foreach (var response in _client.PostStreamingChatCompletionAsync(_request, cancellationToken))
        {
            var choice = response.Choices?.First();
            if (choice == null) continue;

            if (choice.FinishReason != null)
            {
                if (choice.FinishReason == FinishReason.ToolCalls)
                {
                    // Tool Callings
                    foreach (var (_, content) in toolContents)
                    {
                        content.Result = string.IsNullOrWhiteSpace(content.Name)
                            ? FunctionResult.Failed("Function name is required.")
                            : context.Tools != null && context.Tools.TryGetValue(content.Name, out var tool)
                            ? await tool.InvokeAsync(content.Arguments)
                            : FunctionResult.Failed($"Function '{content.Name}' not exist.");
                        context.Messages.AddAssistantMessage(content);
                        yield return new StreamingMessageResponse { Content = content };
                    }

                    await foreach (var stream in StreamingChatCompletionAsync(context, cancellationToken))
                    {
                        yield return stream;
                    };
                }
                else
                {
                    yield return new StreamingMessageResponse
                    {
                        Model = response.Model,
                        EndReason = choice.FinishReason switch
                        {
                            FinishReason.Stop => MessageEndReason.EndTurn,
                            FinishReason.Length => MessageEndReason.MaxTokens,
                            FinishReason.ContentFilter => MessageEndReason.ContentFilter,
                            _ => null
                        },
                        TokenUsage = new Abstractions.AI.TokenUsage
                        {
                            TotalTokens = response.Usage?.TotalTokens,
                            InputTokens = response.Usage?.PromptTokens,
                            OutputTokens = response.Usage?.CompletionTokens
                        }
                    };
                }
            }
            else if (choice.Delta?.Content != null)
            {
                yield return new StreamingMessageResponse
                {
                    Content = new TextContent { Text = choice.Delta.Content }
                };
            }
            else if (choice.Delta?.ToolCalls != null)
            {
                // Tool Call Generation
                var toolCall = choice.Delta.ToolCalls.First();
                if (toolCall == null || toolCall.Index == null)
                    throw new InvalidOperationException("Not Expected Tool Call Object.");

                if (toolContents.TryGetValue((int)toolCall.Index, out var content))
                {
                    content.Arguments ??= new FunctionArguments();
                    content.Arguments.Append(toolCall.Function?.Arguments);
                }
                else
                {
                    content = new ToolContent
                    {
                        Index = (int)toolCall.Index,
                        Id = toolCall.ID,
                        Name = toolCall.Function?.Name,
                        Arguments = new FunctionArguments(toolCall.Function?.Arguments ?? string.Empty)
                    };
                    toolContents.Add((int)toolCall.Index, content);
                }
                yield return new StreamingMessageResponse { Content = content };
            }
            else
            {
                continue;
            }
        }
    }

    private static ChatCompletionRequest ConvertToRequest(MessageContext context)
    {
        // Reasoning Models, 일부 파라미터 작동 안함
        var reason = context.Model.Contains("o1") || context.Model.Contains("o3");

        var request = new ChatCompletionRequest
        {
            Model = context.Model,
            Messages = context.Messages.ToOpenAI(context.System).ToArray(),
            MaxCompletionTokens = context.Parameters?.MaxTokens,
            Temperature = reason ? null : context.Parameters?.Temperature,
            TopP = reason ? null : context.Parameters?.TopP,
            Stop = context.Parameters?.StopSequences?.ToArray(),
        };

        if (context.Tools != null && context.Tools.Count > 0)
        {
            request.Tools = context.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.ToJsonSchema()
                }
            }).ToArray();
        }

        return request;
    }
}
