using Raggle.Connector.OpenAI.ChatCompletion;
using Raggle.Connector.OpenAI.Configurations;
using System.Text.Json;
using Raggle.Abstractions.Tools;
using Raggle.Connector.OpenAI.ChatCompletion.Models;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Connector.OpenAI;

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

    public async Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync()
    {
        var models = await _client.GetChatCompletionModelsAsync();
        return models.Select(m => new ChatCompletionModel
        {
            ModelId = m.ID,
            CreatedAt = m.Created,
            Owner = "OpenAI"
        });
    }

    public async Task<Abstractions.AI.ChatCompletionResponse> ChatCompletionAsync(ChatHistory history, ChatCompletionOptions options)
    {
        var request = BuildChatCompletionRequest(history, options);
        var response = await _client.PostChatCompletionAsync(request);

        var choice = response.Choices?.First();
        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            var cloneHistory = history.Clone();
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var content = new ToolContentBlock 
                { 
                    ID = toolCall.ID,
                    Name = toolCall.Function?.Name,
                    Arguments = toolCall.Function?.Arguments,
                };

                var function = options.Tools?.FirstOrDefault(t => t.Name == content.Name);
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
            return await ChatCompletionAsync(cloneHistory, options);
        }
        else
        {
            var textContent = new TextContentBlock { Text = choice?.Message?.Content ?? string.Empty };
            var totalTokens = response.Usage?.TotalTokens;
            var contents = history.TryGetLastAssistantMessage(out var lastMessage)
                ? lastMessage.Contents.Append(textContent)
                : [ textContent ];

            if (choice?.FinishReason == FinishReason.Length)
                return Abstractions.AI.ChatCompletionResponse.Limit((IContentBlock[])contents.ToArray(), totalTokens);
            else
                return Abstractions.AI.ChatCompletionResponse.Stop((IContentBlock[])contents.ToArray(), totalTokens);
        }
    }

    public async IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(ChatHistory history, ChatCompletionOptions options)
    {
        var request = BuildChatCompletionRequest(history, options);
        var tools = options.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
        var toolContents = new Dictionary<int, ToolContentBlock>();

        await foreach (var response in _client.PostStreamingChatCompletionAsync(request))
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
                var cloneHistory = history.Clone();
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

                await foreach (var stream in StreamingChatCompletionAsync(cloneHistory, options))
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

    private static ChatCompletionRequest BuildChatCompletionRequest(ChatHistory history, ChatCompletionOptions options)
    {
        var messages = new List<Message>();
        if (!string.IsNullOrWhiteSpace(options.System))
        {
            messages.Add(new SystemMessage { Content = options.System });
        }

        foreach (var message in history)
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

        var request = new ChatCompletionRequest
        {
            Model = options.ModelId,
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            TopP = options.TopP,
            Stop = options.StopSequences,
            Messages = messages.ToArray()
        };

        if (options.Tools != null && options.Tools.Length > 0)
        {
            var tools = new List<Tool>();
            foreach (var tool in options.Tools)
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
            request.Tools = tools.ToArray();
        }

        return request;
    }
}
