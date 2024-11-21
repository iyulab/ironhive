using Raggle.Driver.OpenAI.ChatCompletion;
using Raggle.Driver.OpenAI.Configurations;
using System.Text.Json;
using Raggle.Abstractions.Tools;
using Raggle.Driver.OpenAI.ChatCompletion.Models;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using System.Runtime.CompilerServices;

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
        var openaiRequest = ConvertToOpenAIRequest(request);
        var response = await _client.PostChatCompletionAsync(openaiRequest, cancellationToken);

        var choice = response.Choices?.First();
        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            var cloneHistory = request.Messages.Clone();
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var content = new ToolContentBlock
                {
                    ID = toolCall.ID,
                    Name = toolCall.Function?.Name,
                    Arguments = toolCall.Function?.Arguments,
                };

                var function = request.Tools?.FirstOrDefault(t => t.Name == content.Name);
                if (function == null)
                {
                    content.Result = FunctionResult.Failed($"Function '{content.Name}' not exist.");
                }
                else
                {
                    var result = await function.InvokeAsync(content.Arguments);
                    content.Result = result;
                }
                cloneHistory.AddAssistantMessage(content);
            }
            request.Messages = cloneHistory;
            return await ChatCompletionAsync(request, cancellationToken);
        }
        else
        {
            var textContent = new TextContentBlock { Text = choice?.Message?.Content ?? string.Empty };
            var contents = request.Messages.TryGetLastAssistantMessage(out var lastMessage)
                ? lastMessage.Contents.Append(textContent)
                : [textContent];
            var tokenUsage = new Abstractions.AI.TokenUsage
            {
                TotalTokens = response.Usage?.TotalTokens,
                InputTokens = response.Usage?.PromptTokens,
                OutputTokens = response.Usage?.CompletionTokens
            };

            if (choice?.FinishReason == FinishReason.Length)
            {
                return new Abstractions.AI.ChatCompletionResponse
                { 
                    Completed = false,
                    Contents = contents.ToArray(),
                    TokenUsage = tokenUsage,
                    ErrorMessage = "You have reached the token limit.",
                };
            }
            else
            {
                return new Abstractions.AI.ChatCompletionResponse
                {
                    Completed = true,
                    Contents = contents.ToArray(),
                    TokenUsage = tokenUsage,
                };
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(
        Abstractions.AI.ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var openaiRequest = ConvertToOpenAIRequest(request);
        var tools = request.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
        var toolContents = new Dictionary<int, ToolContentBlock>();

        await foreach (var response in _client.PostStreamingChatCompletionAsync(openaiRequest, cancellationToken))
        {
            var choice = response.Choices?.First();
            if (choice == null) continue;

            if (choice.FinishReason == FinishReason.Stop)
            {
                // Text Stream Stop
                yield return new StreamingStopResponse();
            }
            else if (choice.FinishReason == FinishReason.ContentFilter)
            {
                // Text Invalid Content Filter
                yield return new StreamingFilterResponse();
            }
            else if (choice.FinishReason == FinishReason.Length)
            {
                // Token Limit
                yield return new StreamingLimitResponse();
            }
            else if (choice.FinishReason == FinishReason.ToolCalls)
            {
                // Tool Callings
                var cloneHistory = request.Messages.Clone();
                foreach (var (_, content) in toolContents)
                {
                    yield return new StreamingToolUseResponse { Name = content.Name, Argument = content.Arguments };
                    if (string.IsNullOrWhiteSpace(content.Name))
                    {
                        content.Result = FunctionResult.Failed("Function name is required.");
                    }
                    else if (tools.TryGetValue(content.Name, out var tool))
                    {
                        var result = await tool.InvokeAsync(content.Arguments);
                        content.Result = result;
                    }
                    else
                    {
                        content.Result = FunctionResult.Failed($"Function '{content.Name}' not exist.");
                    }
                    cloneHistory.AddAssistantMessage(content);
                    yield return new StreamingToolResultResponse { Name = content.Name, Result = content.Result };
                }

                request.Messages = cloneHistory;
                await foreach (var stream in StreamingChatCompletionAsync(request, cancellationToken))
                {
                    yield return stream;
                };
            }
            else if (choice.Delta?.Content != null)
            {
                // Text Generation
                var text = choice.Delta.Content ?? string.Empty;
                yield return new StreamingTextResponse { Text = text };
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
                    content = new ToolContentBlock
                    {
                        ID = toolCall.ID,
                        Name = toolCall.Function?.Name,
                        Arguments = toolCall.Function?.Arguments ?? string.Empty
                    };
                    toolContents.Add((int)toolCall.Index, content);
                }
                yield return new StreamingToolCallResponse { Name = content.Name, Argument = content.Arguments };
            }
        }
    }

    #region Private Methods

    private static ChatCompletion.Models.ChatCompletionRequest ConvertToOpenAIRequest(Abstractions.AI.ChatCompletionRequest request)
    {
        var messages = new List<Message>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            messages.Add(new SystemMessage { Content = request.System });
        }

        foreach (var message in request.Messages)
        {
            if (message.Contents == null || message.Contents.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var contents = new List<MessageContent>();
                foreach (var content in message.Contents)
                {
                    if (content is TextContentBlock text)
                    {
                        contents.Add(new TextMessageContent { Text = text.Text ?? string.Empty });
                    }
                    else if (content is ImageContentBlock image)
                    {
                        contents.Add(new ImageMessageContent { ImageURL = new ImageURL { URL = image.Data ?? string.Empty } });
                    }
                }
                messages.Add(new UserMessage { Content = contents });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var content in message.Contents)
                {
                    if (content is TextContentBlock text)
                    {
                        messages.Add(new AssistantMessage { Content = text.Text });
                    }
                    else if (content is ToolContentBlock tool)
                    {
                        messages.Add(new AssistantMessage
                        {
                            ToolCalls = [
                                new ToolCall
                                {
                                    ID = tool.ID,
                                    Function = new FunctionCall
                                    {
                                        Name = tool.Name,
                                        Arguments = JsonSerializer.Serialize(tool.Arguments)
                                    }
                                }
                            ]
                        });
                        messages.Add(new ToolMessage 
                        { 
                            ID = tool.ID ?? string.Empty, 
                            Content = JsonSerializer.Serialize(tool.Result)
                        });
                    }
                }
            }
        }

        var openaiRequest = new ChatCompletion.Models.ChatCompletionRequest
        {
            Model = request.Model,
            Messages = messages.ToArray(),
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            Stop = request.StopSequences,
        };

        if (request.Tools != null && request.Tools.Length > 0)
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
            openaiRequest.Tools = tools.ToArray();
        }

        return openaiRequest;
    }

    #endregion
}
