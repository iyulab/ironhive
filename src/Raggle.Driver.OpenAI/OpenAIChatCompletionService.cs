using Raggle.Driver.OpenAI.ChatCompletion;
using Raggle.Driver.OpenAI.Configurations;
using System.Text.Json;
using Raggle.Abstractions.Tools;
using Raggle.Driver.OpenAI.ChatCompletion.Models;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using System.Runtime.CompilerServices;
using System.Diagnostics;

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
    public async Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetChatCompletionModelsAsync(cancellationToken);
        return models.Select(m => new ChatCompletionModel
        {
            Model = m.ID,
            CreatedAt = m.Created,
            Owner = m.OwnedBy,
        });
    }

    /// <inheritdoc />
    public async Task<Abstractions.AI.ChatCompletionResponse> ChatCompletionAsync(
        Abstractions.AI.ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOpenAIRequest(request);
        var response = await _client.PostChatCompletionAsync(_request, cancellationToken);
        var choice = response.Choices?.First();

        var content = request.Messages.Last().Role == MessageRole.Assistant
            ? request.Messages.Last().Content
            : new MessageContentCollection();

        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var index = toolCall.Index ?? 0;
                var id = toolCall.ID;
                var name = toolCall.Function?.Name;
                var args = toolCall.Function?.Arguments;

                var result = string.IsNullOrWhiteSpace(name)
                    ? FunctionResult.Failed("Function name is required.")
                    : request.Tools != null && request.Tools.TryGetValue(name, out var function)
                    ? await function.InvokeAsync(args)
                    : FunctionResult.Failed($"Function '{name}' not exist.");

                content.AddTool(id, name, args, result);
            }
            request.Messages.AddAssistantMessage(content);
            return await ChatCompletionAsync(request, cancellationToken);
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

            var result = new Abstractions.AI.ChatCompletionResponse
            {
                EndReason = choice?.FinishReason switch
                {
                    FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                    FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                    FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
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
    public async IAsyncEnumerable<Abstractions.AI.ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        Abstractions.AI.ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOpenAIRequest(request);
        var toolContents = new Dictionary<int, ToolContent>();

        await foreach (var response in _client.PostStreamingChatCompletionAsync(_request, cancellationToken))
        {
            var choice = response.Choices?.First();
            if (choice == null) continue;

            if (choice.FinishReason == FinishReason.ToolCalls)
            {
                // Tool Callings
                foreach (var (_, content) in toolContents)
                {
                    content.Result = string.IsNullOrWhiteSpace(content.Name)
                        ? FunctionResult.Failed("Function name is required.")
                        : request.Tools != null && request.Tools.TryGetValue(content.Name, out var tool)
                        ? await tool.InvokeAsync(content.Arguments)
                        : FunctionResult.Failed($"Function '{content.Name}' not exist.");
                    request.Messages.AddAssistantMessage(content);
                    yield return new ChatCompletionStreamingResponse { Content = content };
                }

                await foreach (var stream in StreamingChatCompletionAsync(request, cancellationToken))
                {
                    yield return stream;
                };
            }
            else if (choice.Delta?.Content != null)
            {
                yield return new ChatCompletionStreamingResponse
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
                    content.Arguments += toolCall.Function?.Arguments ?? string.Empty;
                }
                else
                {
                    content = new ToolContent
                    {
                        Index = (int)toolCall.Index,
                        Id = toolCall.ID,
                        Name = toolCall.Function?.Name,
                        Arguments = toolCall.Function?.Arguments ?? string.Empty
                    };
                    toolContents.Add((int)toolCall.Index, content);
                }
                yield return new ChatCompletionStreamingResponse { Content = content };
            }
            else if(choice.FinishReason != null)
            {
                yield return new ChatCompletionStreamingResponse
                {
                    Model = response.Model,
                    EndReason = choice.FinishReason switch
                    {
                        FinishReason.Stop => ChatCompletionEndReason.EndTurn,
                        FinishReason.Length => ChatCompletionEndReason.MaxTokens,
                        FinishReason.ContentFilter => ChatCompletionEndReason.ContentFilter,
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
    }

    #region Private Methods

    private static ChatCompletion.Models.ChatCompletionRequest ConvertToOpenAIRequest(
        Abstractions.AI.ChatCompletionRequest request)
    {
        var _request = new ChatCompletion.Models.ChatCompletionRequest
        {
            Model = request.Model,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            Stop = request.StopSequences,
            Messages = [],
        };

        var _messages = new List<ChatCompletion.Models.Message>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            _messages.Add(new SystemMessage { Content = request.System });
        }

        foreach (var message in request.Messages)
        {
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var content = new List<MessageContent>();
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        content.Add(new TextMessageContent { Text = text.Text ?? string.Empty });
                    }
                    else if (item is ImageContent image)
                    {
                        content.Add(new ImageMessageContent { ImageURL = new ImageURL { URL = image.Data ?? string.Empty } });
                    }
                }
                _messages.Add(new UserMessage { Content = content });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        _messages.Add(new AssistantMessage { Content = text.Text });
                    }
                    else if (item is ToolContent tool)
                    {
                        _messages.Add(new AssistantMessage
                        {
                            ToolCalls = [
                                new ToolCall
                                {
                                    ID = tool.Id,
                                    Function = new FunctionCall
                                    {
                                        Name = tool.Name,
                                        Arguments = JsonSerializer.Serialize(tool.Arguments)
                                    }
                                }
                            ]
                        });
                        _messages.Add(new ToolMessage 
                        { 
                            ID = tool.Id ?? string.Empty, 
                            Content = JsonSerializer.Serialize(tool.Result)
                        });
                    }
                }
            }
        }
        _request.Messages = _messages.ToArray();

        if (request.Tools != null && request.Tools.Count > 0)
        {
            var tools = new List<Tool>();
            foreach (var tool in request.Tools)
            {
                var schema = tool.GetParametersJsonSchema();
                tools.Add(new Tool
                {
                    Function = new Function
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = new InputSchema
                        {
                            Properties = schema.Properties,
                            Required = schema.Required,
                        }
                    }
                });
            }
            _request.Tools = tools.ToArray();
        }

        return _request;
    }

    #endregion
}
