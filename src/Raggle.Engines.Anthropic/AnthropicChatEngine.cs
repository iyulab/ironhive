using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Models;
using Raggle.Abstractions.Tools;
using Raggle.Engines.Anthropic.ChatCompletion;
using Raggle.Engines.Anthropic.Configurations;

namespace Raggle.Engines.Anthropic;

public class AnthropicChatEngine : IChatEngine
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatEngine(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatEngine(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    public Task<IEnumerable<ChatModel>> GetChatCompletionModelsAsync()
    {
        var models = _client.GetChatCompletionModels();
        var chatCompletionModels = models.Select(m => new ChatModel
        {
            ModelId = m.ModelId,
            Owner = "Anthropic"
        });
        return Task.FromResult(chatCompletionModels);
    }

    public async Task<ChatResponse> ChatCompletionAsync(ChatHistory history, ChatOptions options)
    {
        var request = BuildMessagesRequest(history, options);
        var response = await _client.PostMessagesAsync(request);
        return new ChatResponse();
    }

    public IAsyncEnumerable<StreamingChatResponse> StreamingChatCompletionAsync(ChatHistory history, ChatOptions options)
    {
        throw new NotImplementedException();
    }

    private MessagesRequest BuildMessagesRequest(ChatHistory history, ChatOptions options)
    {
        var request = new MessagesRequest
        {
            Model = options.ModelId,
            MaxTokens = options.MaxTokens,
            System = options.System,
            Messages = ConvertChatHistory(history),
            Temperature = options.Temperature,
            TopK = options.TopK,
            TopP = options.TopP,
            StopSequences = options.StopSequences,
        };

        if (options.Tools != null && options.Tools.Length > 0)
        {
            request.Tools = ConvertFunctionTools(options.Tools);
        }

        return request;
    }

    private static Message[] ConvertChatHistory(ChatHistory history)
    {
        var messages = new List<Message>();
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
                        contents.Add(new ImageMessageContent { Source = new ImageSource { Data = image.Data, MediaType = "image/jpg" } });
                    }
                }
                messages.Add(new Message { Role = "user", Content = contents.ToArray() });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                var contents = new List<MessageContent>();
                foreach (var content in message.Contents)
                {
                    if (content is TextContentBlock text)
                    {
                        contents.Add(new TextMessageContent { Text = text.Text ?? string.Empty });
                        messages.Add(new Message { Role = "assistant", Content = contents.ToArray() });
                    }
                    else if (content is ToolContentBlock tool)
                    {
                        messages.Add(new Message { Role = "assistant", Content = contents.ToArray() });
                        messages.Add(new Message { Role = "user", Content = contents.ToArray() });
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
                Name = tool.Name,
                Description = tool.Description,
                InputSchema = new InputSchema
                {
                    Properties = tool.Properties,
                    Required = tool.Required
                }
            });
        }
        return tools.ToArray();
    }
}
