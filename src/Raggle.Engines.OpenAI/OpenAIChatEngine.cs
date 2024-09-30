using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Models;
using Raggle.Engines.OpenAI.ChatCompletion;
using Raggle.Engines.OpenAI.Configurations;
using System.Text.Json;
using Raggle.Abstractions.Tools;
using System.Runtime.ExceptionServices;
using System.ComponentModel.DataAnnotations;

namespace Raggle.Engines.OpenAI;

public class OpenAIChatEngine : IChatEngine
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatEngine(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
    }

    public OpenAIChatEngine(string apiKey)
    {
        _client = new OpenAIChatCompletionClient(apiKey);
    }

    public async Task<IEnumerable<ChatModel>> GetChatCompletionModelsAsync()
    {
        var models = await _client.GetChatCompletionModelsAsync();
        return models.Select(m => new ChatModel
        {
            ModelId = m.ID,
            CreatedAt = m.Created,
            Owner = "OpenAI"
        });
    }

    public async Task<ChatResponse> ChatCompletionAsync(ChatHistory history, ChatOptions options)
    {
        var request = BuildChatCompletionRequest(history, options);
        var response = await _client.PostChatCompletionAsync(request);

        var choice = response.Choices?.First();
        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            var message = new ChatMessage { Role = MessageRole.Assistant, Contents = [] };
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var content = new ToolContentBlock 
                { 
                    ID = toolCall.ID,
                    Name = toolCall.Function?.Name
                };

                var function = options.Tools?.FirstOrDefault(t => t.Name == content.Name);
                var stringArgs = toolCall.Function?.Arguments;
                if (function == null)
                {
                    content.Result = FunctionResult.Failed($"Function '{content.Name}' not exist.");
                }
                else if (string.IsNullOrWhiteSpace(stringArgs))
                {
                    content.Result = await function.InvokeAsync(null);
                }
                else
                {
                    var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(stringArgs);
                    var result = await function.InvokeAsync(arguments);
                    content.Result = result;
                }
                message.Contents.Add(content);
            }
            history.Add(message);
            return await ChatCompletionAsync(history, options);
        }
        else if (choice?.FinishReason == FinishReason.Stop && choice.Message?.Content != null)
        {
            return new ChatResponse();
        }
        else
        {
            return new ChatResponse();
        }
    }

    public async IAsyncEnumerable<StreamingChatResponse> StreamingChatCompletionAsync(ChatHistory history, ChatOptions options)
    {
        var request = BuildChatCompletionRequest(history, options);
        var tools = options.Tools?.ToDictionary(t => t.Name, t => t);
        int index = 0;
        TextContentBlock? textContent = null;
        ToolContentBlock? toolContent = null;

        var contents = new List<MessageContent>();

        await foreach (var response in _client.PostStreamingChatCompletionAsync(request))
        {
            Console.WriteLine($"Current Index: {index}");
            Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));
            var choice = response.Choices?.First();
            if (choice == null) continue;

            if (choice.FinishReason == FinishReason.Stop)
            {
                yield return StreamingChatResponse.Text(textContent);
                break;
            }
            if (choice.Delta?.Content != null)
            {
                textContent ??= new TextContentBlock();
                textContent.Text += choice.Delta.Content;
                yield return StreamingChatResponse.Text(textContent);
            }

            if (choice.FinishReason == FinishReason.ToolCalls)
            {
                if (tools.TryGetValue(toolContent.Name, out var function))
                {
                    if (toolContent.Arguments is string strArgs)
                    {
                        var args = JsonSerializer.Deserialize<Dictionary<string, object>>(strArgs);
                        var result = await function.InvokeAsync(args);
                        toolContent.Result = result;
                        history.Add(new ChatMessage { Role = MessageRole.Assistant, Contents = [toolContent] });
                        StreamingChatResponse.Tool(toolContent);
                        await foreach (var res2 in StreamingChatCompletionAsync(history, options))
                        {
                            yield return res2;
                        }
                    }
                }
            }
            if (choice.Delta?.ToolCalls != null)
            {
                toolContent ??= new ToolContentBlock();
                toolContent.ID ??= choice.Delta.ToolCalls[0].ID;
                toolContent.Name ??= choice.Delta.ToolCalls[0].Function?.Name;
                toolContent.Arguments += choice.Delta.ToolCalls[0].Function?.Arguments ?? string.Empty;
                yield return StreamingChatResponse.Tool(toolContent);
            }
        }

        yield return StreamingChatResponse.Stop();
    }

    private async IAsyncEnumerable<StreamingChatResponse> StreamingTextAsync(ChoiceDelta delta)
    {
        yield return StreamingChatResponse.Stop();
    }

    private async IAsyncEnumerable<StreamingChatResponse> StreamingToolAsync(ChoiceDelta delta)
    {
        yield return StreamingChatResponse.Stop();
    }

    private ChatCompletionRequest BuildChatCompletionRequest(ChatHistory history, ChatOptions options)
    {
        var request = new ChatCompletionRequest
        {
            Model = options.ModelId,
            MaxTokens = options.MaxTokens,
            Messages = ConvertChatSession(history, options.System),
            Temperature = options.Temperature,
            TopP = options.TopP,
            Stop = options.StopSequences,
        };

        if (options.Tools != null && options.Tools.Length > 0)
        {
            request.Tools = ConvertFunctionTools(options.Tools);
        }

        return request;
    }

    private static Message[] ConvertChatSession(ChatHistory history, string? system)
    {
        var messages = new List<Message>();
        if (!string.IsNullOrWhiteSpace(system))
        {
            messages.Add(new SystemMessage { Content = system });
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
                var index = 0;
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
                                    Index = index,
                                    ID = tool.ID,
                                    Function = new FunctionCall
                                    {
                                        Name = tool.Name,
                                        Arguments = JsonSerializer.Serialize(tool.Arguments)
                                    }
                                }
                            ]
                        });
                        messages.Add(new ToolMessage { ID = tool.ID ?? string.Empty, Content = JsonSerializer.Serialize(tool.Result) });
                    }
                    index++;
                }
            }
        }
        return messages.ToArray();
    }

    private Tool[] ConvertFunctionTools(FunctionTool[] functionTools)
    {
        var tools = new List<Tool>();
        foreach (var tool in functionTools)
        {
            tools.Add(new Tool
            {
                Function = new Function
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Parameters = new InputSchema
                    {
                        Properties = tool.Properties,
                        Required = tool.Required
                    }
                }
            });
        }
        return tools.ToArray();
    }
}
