using Raggle.Driver.OpenAI.ChatCompletion;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Abstractions.Tools;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Driver.OpenAI.Extensions;
using System.Runtime.CompilerServices;
using ChatCompletionRequest = Raggle.Abstractions.AI.ChatCompletionRequest;
using OpenAIChatCompletionRequest = Raggle.Driver.OpenAI.ChatCompletion.ChatCompletionRequest;
using AssistantMessage = Raggle.Abstractions.Messages.AssistantMessage;
using OpenAIAssistantMessage = Raggle.Driver.OpenAI.ChatCompletion.AssistantMessage;

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
    public async Task<ChatCompletionResponse<IMessage>> GenerateMessageAsync(
        ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOpenAI(request);
        var response = await _client.PostChatCompletionAsync(_request, cancellationToken);
        var choice = response.Choices?.First();
        var content = new MessageContentCollection();

        if (choice?.Message?.ToolCalls?.Count > 0)
        {
            foreach (var t in choice.Message.ToolCalls)
            {
                content.AddTool(
                    t.ID, 
                    t.Function?.Name, 
                    new ToolArguments(t.Function?.Arguments),
                    null);
            }
        }
        if (!string.IsNullOrWhiteSpace(choice?.Message?.Content))
        {
            content.AddText(choice.Message.Content);
        }

        var result = new ChatCompletionResponse<IMessage>
        {
            EndReason = choice?.FinishReason switch
            {
                FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
                FinishReason.ToolCalls => ChatCompletionEndReason.ToolUse,
                _ => null
            },
            TokenUsage = new Abstractions.AI.TokenUsage
            {
                TotalTokens = response.Usage?.TotalTokens,
                InputTokens = response.Usage?.PromptTokens,
                OutputTokens = response.Usage?.CompletionTokens
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
        var _request = ConvertToOpenAI(request);

        await foreach (var response in _client.PostStreamingChatCompletionAsync(_request, cancellationToken))
        {
            var choice = response.Choices?.First();
            if (choice == null) continue;

            if (choice.FinishReason != null)
            {
                yield return new ChatCompletionResponse<IMessageContent>
                {
                    EndReason = choice.FinishReason switch
                    {
                        FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                        FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                        FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
                        FinishReason.ToolCalls => ChatCompletionEndReason.ToolUse,
                        _ => null
                    },
                    TokenUsage = new Abstractions.AI.TokenUsage
                    {
                        TotalTokens = response.Usage?.TotalTokens,
                        InputTokens = response.Usage?.PromptTokens,
                        OutputTokens = response.Usage?.CompletionTokens
                    },
                };
            }
            else if (choice.Delta?.Content != null)
            {
                yield return new ChatCompletionResponse<IMessageContent>
                {
                    Content = new TextContent
                    {
                        Index = null,
                        Text = choice.Delta.Content
                    }
                };
            }
            else if (choice.Delta?.ToolCalls?.First() != null)
            {
                var t = choice.Delta.ToolCalls.First();

                yield return new ChatCompletionResponse<IMessageContent>
                {
                    Content = new ToolContent
                    {
                        Index = t.Index,
                        Id = t.ID,
                        Name = t.Function?.Name,
                        Arguments = new ToolArguments(t.Function?.Arguments)
                    }
                };
            }
            else
            {
                continue;
            }
        }
    }

    private static OpenAIChatCompletionRequest ConvertToOpenAI(
        ChatCompletionRequest request)
    {
        // Reasoning Models, 일부 파라미터 작동 안함
        var reason = request.Model.Contains("o1") || request.Model.Contains("o3");

        var _request = new OpenAIChatCompletionRequest
        {
            Model = request.Model,
            Messages = request.Messages.ToOpenAI(request.System),
            MaxCompletionTokens = request.MaxTokens,
            Temperature = reason ? null : request.Temperature,
            TopP = reason ? null : request.TopP,
            Stop = request.StopSequences,
        };

        if (request.Tools != null && request.Tools.Count > 0)
        {
            _request.Tools = request.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = new
                    {
                        Properties = t.Parameters,
                        Required = t.Required,
                    }
                }
            });

            if (request.ToolChoice != null)
            {
                if (request.ToolChoice.Mode == ToolChoiceMode.Manual)
                {
                    _request.ToolChoice = new RequiredToolChoice
                    {
                        Function = new FunctionChoice
                        {
                            Name = request.ToolChoice.ToolName,
                        }
                    };
                }
                else if (request.ToolChoice.Mode == ToolChoiceMode.Auto)
                {
                    _request.ToolChoice = new AutoToolChoice();
                }
                else if (request.ToolChoice.Mode == ToolChoiceMode.None)
                {
                    _request.ToolChoice = new NoneToolChoice();
                }
            }
        }

        return _request;
    }
}
