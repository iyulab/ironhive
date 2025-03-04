using Raggle.Connectors.OpenAI.Configurations;
using Raggle.Connectors.OpenAI.Extensions;
using System.Runtime.CompilerServices;
using AssistantMessage = Raggle.Abstractions.ChatCompletion.Messages.AssistantMessage;
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
    public async Task<ChatCompletionResult<IMessage>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages, options);
        var response = await _client.PostChatCompletionAsync(request, cancellationToken);
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

        var result = new ChatCompletionResult<IMessage>
        {
            EndReason = choice?.FinishReason switch
            {
                FinishReason.ToolCalls => EndReason.ToolCall,
                FinishReason.Stop => EndReason.EndTurn,
                FinishReason.Length => EndReason.MaxTokens,
                FinishReason.ContentFilter => EndReason.ContentFilter,
                _ => null
            },
            TokenUsage = new TokenUsage
            {
                TotalTokens = response.Usage?.TotalTokens,
                InputTokens = response.Usage?.PromptTokens,
                OutputTokens = response.Usage?.CompletionTokens
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
        var request = BuildRequest(messages, options);

        await foreach (var response in _client.PostStreamingChatCompletionAsync(request, cancellationToken))
        {
            var choice = response.Choices?.FirstOrDefault();
            if (choice == null) continue;

            if (choice.FinishReason != null)
            {
                yield return new ChatCompletionResult<IMessageContent>
                {
                    EndReason = choice.FinishReason switch
                    {
                        FinishReason.ToolCalls => EndReason.ToolCall,
                        FinishReason.Stop => EndReason.EndTurn,
                        FinishReason.Length => EndReason.MaxTokens,
                        FinishReason.ContentFilter => EndReason.ContentFilter,
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
                yield return new ChatCompletionResult<IMessageContent>
                {
                    Data = new TextContent
                    {
                        Index = null,
                        Text = choice.Delta.Content
                    }
                };
            }
            else if (choice.Delta?.ToolCalls?.First() != null)
            {
                var t = choice.Delta.ToolCalls.First();

                yield return new ChatCompletionResult<IMessageContent>
                {
                    Data = new ToolContent
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

    private static ChatCompletionRequest BuildRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        // Reasoning Models, 일부 파라미터 작동 안함
        var reason = options.Model.Contains("o1") || options.Model.Contains("o3");

        var request = new ChatCompletionRequest
        {
            Model = options.Model,
            Messages = messages.ToOpenAI(options.System),
            MaxCompletionTokens = options.MaxTokens,
            Temperature = reason ? null : options.Temperature,
            TopP = reason ? null : options.TopP,
            Stop = options.StopSequences,
        };

        if (options.Tools != null && options.Tools.Count > 0)
        {
            request.Tools = options.Tools.Select(t => new Tool
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

            if (options.ToolChoice != null)
            {
                if (options.ToolChoice is ManualToolChoice manual)
                {
                    request.ToolChoice = new RequiredToolChoice
                    {
                        Function = new FunctionChoice
                        {
                            Name = manual.ToolName,
                        }
                    };
                }
                else if (options.ToolChoice is AutoToolChoice)
                {
                    request.ToolChoice = new OpenAIAutoToolChoice();
                }
                else if (options.ToolChoice is DisabledToolChoice)
                {
                    request.ToolChoice = new NoneToolChoice();
                }
            }
        }

        return request;
    }
}
