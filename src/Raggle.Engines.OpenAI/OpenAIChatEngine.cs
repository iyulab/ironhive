using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Models;
using Raggle.Engines.OpenAI.ChatCompletion;
using Raggle.Engines.OpenAI.Configurations;
using System.Text.Json;
using Raggle.Abstractions.Tools;

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

    public async Task<ChatResponse> ChatCompletionAsync(ChatSession session, ChatOptions options)
    {
        var request = BuildChatCompletionRequest(session, options);
        var response = await _client.PostChatCompletionAsync(request);

        var choice = response.Choices?.First();
        if (choice?.FinishReason == FinishReason.ToolCalls && choice.Message?.ToolCalls?.Length > 0)
        {
            var message = new ChatMessage { Role = ChatRole.Assistant, Contents = [] };
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var content = new ToolContentBlock 
                { 
                    Index = toolCall.Index,
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
                    var jsonArguments = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stringArgs);
                    var arguments = new Dictionary<string, object?>();
                    foreach (var arg in jsonArguments)
                    {
                        if (arg.Value.ValueKind == JsonValueKind.String)
                        {
                            arguments.Add(arg.Key, arg.Value.GetString());
                        }
                        else if (arg.Value.ValueKind == JsonValueKind.Number)
                        {
                            arguments.Add(arg.Key, arg.Value.GetInt32());
                        }
                        else if (arg.Value.ValueKind == JsonValueKind.True || arg.Value.ValueKind == JsonValueKind.False)
                        {
                            arguments.Add(arg.Key, arg.Value.GetBoolean());
                        }
                        else if (arg.Value.ValueKind == JsonValueKind.Object)
                        {
                            arguments.Add(arg.Key, arg.Value);
                        }
                    }
                    var result = await function.InvokeAsync(arguments);
                    content.Result = result;
                }
                message.Contents.Add(content);
            }
            session.Add(message);
            return await ChatCompletionAsync(session, options);
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
    
    public async IAsyncEnumerable<StreamingChatResponse> StreamingChatCompletionAsync(ChatSession session, ChatOptions options)
    {
        var request = BuildChatCompletionRequest(session, options);
        int index = 0;
        TextContentBlock? textContent = null;
        ToolContentBlock? toolContent = null;

        await foreach (var response in _client.PostStreamingChatCompletionAsync(request))
        {
            Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));
            var choice = response.Choices?.First();
            if (choice == null) continue;
            
            if (choice.FinishReason == FinishReason.ToolCalls)
            {

            }

            if (choice.Delta?.ToolCalls != null)
            {
                if (toolContent == null)
                {
                    toolContent = new ToolContentBlock { Index = index };
                }
            }

            yield return new StreamingChatResponse();
        }
    }

    private ChatCompletionRequest BuildChatCompletionRequest(ChatSession session, ChatOptions options)
    {
        var request = new ChatCompletionRequest
        {
            Model = options.ModelId,
            MaxTokens = options.MaxTokens,
            Messages = SessionToMessage(session, options.System),
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

    private static Message[] SessionToMessage(ChatSession session, string? system)
    {
        var messages = new List<Message>();
        if (!string.IsNullOrWhiteSpace(system))
        {
            messages.Add(new SystemMessage { Content = system });
        }

        foreach (var message in session)
        {
            if (message.Contents == null || message.Contents.Count == 0)
                continue;

            if (message.Role == ChatRole.User)
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
            else if (message.Role == ChatRole.Assistant)
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
                                    Index = tool.Index ?? 0, 
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
