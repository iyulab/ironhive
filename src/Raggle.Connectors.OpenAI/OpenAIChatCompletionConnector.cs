using Raggle.Connectors.OpenAI.Configurations;
using Raggle.Connectors.OpenAI.Extensions;
using System.Runtime.CompilerServices;
using ChatCompletionRequest = Raggle.Abstractions.ChatCompletion.ChatCompletionRequest;
using OpenAIChatCompletionRequest = Raggle.Connectors.OpenAI.ChatCompletion.ChatCompletionRequest;
using AssistantMessage = Raggle.Abstractions.ChatCompletion.Messages.AssistantMessage;
using OpenAIAssistantMessage = Raggle.Connectors.OpenAI.ChatCompletion.AssistantMessage;
using TokenUsage = Raggle.Abstractions.ChatCompletion.TokenUsage;
using Raggle.Connectors.OpenAI.ChatCompletion;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.ChatCompletion.Tools;
using AutoToolChoice = Raggle.Abstractions.ChatCompletion.Tools.AutoToolChoice;
using OpenAIAutoToolChoice = Raggle.Connectors.OpenAI.ChatCompletion.AutoToolChoice;

namespace Raggle.Connectors.OpenAI;

public class OpenAIChatCompletionConnector : IChatCompletionConnector
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatCompletionConnector(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIChatCompletionConnector(string apiKey)
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
                FinishReason.ToolCalls => ChatCompletionEndReason.ToolCall,
                FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
                _ => null
            },
            TokenUsage = new TokenUsage
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
                        FinishReason.ToolCalls => ChatCompletionEndReason.ToolCall,
                        FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                        FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                        FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
                        _ => null
                    },
                    TokenUsage = new TokenUsage
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
                        t.Required,
                    }
                }
            });

            if (request.ToolChoice != null)
            {
                if (request.ToolChoice is ManualToolChoice manual)
                {
                    _request.ToolChoice = new RequiredToolChoice
                    {
                        Function = new FunctionChoice
                        {
                            Name = manual.ToolName,
                        }
                    };
                }
                else if (request.ToolChoice is AutoToolChoice)
                {
                    _request.ToolChoice = new OpenAIAutoToolChoice();
                }
                else if (request.ToolChoice is DisabledToolChoice)
                {
                    _request.ToolChoice = new NoneToolChoice();
                }
            }
        }

        return _request;
    }
}
