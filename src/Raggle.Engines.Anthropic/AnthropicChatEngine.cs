using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Tools;
using Raggle.Engines.Anthropic.ChatCompletion;
using Raggle.Engines.Anthropic.Configurations;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        var tools = options.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
        var contents = new List<IContentBlock>();

        if (response.StopReason == StopReason.ToolUse)
        {
            foreach (var content in response.Content)
            {
                if (content is TextMessageContent textContent)
                {
                    contents.Add(new TextContentBlock { Text = textContent.Text });
                }
                else if (content is ToolUseMessageContent toolUseContent)
                {
                    var toolContent = new ToolContentBlock
                    { 
                        ID = toolUseContent.ID, 
                        Name = toolUseContent.Name, 
                        Arguments =  toolUseContent.Input?.ToString() ?? string.Empty
                    };

                    if (string.IsNullOrWhiteSpace(toolUseContent.Name))
                    {
                        toolContent.Result = FunctionResult.Failed("Tool name is missing");
                    }
                    else if (tools.TryGetValue(toolUseContent.Name, out var tool))
                    {
                        var result = await tool.InvokeAsync(toolContent.Arguments);
                        toolContent.Result = result;
                    }
                    else
                    {
                        toolContent.Result = FunctionResult.Failed("Tool not found");
                    }
                    contents.Add(toolContent);
                }
            }

            var cloneHistory = history.Clone();
            cloneHistory.AddAssistantMessages(contents);
            return await ChatCompletionAsync(cloneHistory, options);
        }
        else
        {
            if (history.TryGetLastAssistantMessage(out var lastAssistantMessage))
            {
                contents.AddRange(lastAssistantMessage.Contents);
            }

            foreach (var content in response.Content)
            {
                if (content is TextMessageContent textContent)
                {
                    contents.Add(new TextContentBlock { Text = textContent.Text });
                }
                else if (content is ToolUseMessageContent toolContent)
                {
                    contents.Add(new ToolContentBlock { ID = toolContent.ID, Name = toolContent.Name });
                }
            }
            var totalTokens = response.Usage.InputTokens + response.Usage.OutputTokens;

            if (response.StopReason == StopReason.MaxTokens)
                return ChatResponse.Limit(contents.ToArray(), totalTokens);
            else
                return ChatResponse.Stop(contents.ToArray(), totalTokens);
        }
    }

    public async IAsyncEnumerable<IStreamingChatResponse> StreamingChatCompletionAsync(ChatHistory history, ChatOptions options)
    {
        var request = BuildMessagesRequest(history, options);
        var tools = options.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
        var contents = new List<IContentBlock>();

        await foreach (var response in _client.PostStreamingMessagesAsync(request))
        {
            if (response is PingEvent)
            {
                // nothing to do
            }
            else if (response is MessageStartEvent messageStart)
            {
                // nothing to do
            }
            else if (response is ErrorEvent error)
            {
                yield return new StreamingErrorResponse { Message = error.ToString() };
            }
            else if (response is MessageStopEvent)
            {
                // nothing to do
            }
            else if (response is MessageDeltaEvent messageDelta)
            {
                var reason = messageDelta.Delta?.StopReason;
                if (reason == StopReason.EndTurn || reason == StopReason.StopSequence)
                {
                    yield return new StreamingStopResponse();
                }
                else if (reason == StopReason.MaxTokens)
                {
                    yield return new StreamingLimitResponse();
                }
                else if (reason == StopReason.ToolUse)
                {
                    foreach (var content in contents)
                    {
                        if (content is ToolContentBlock toolContent)
                        {
                            if (string.IsNullOrWhiteSpace(toolContent.Name))
                            {
                                toolContent.Result = FunctionResult.Failed("Tool name is missing");
                            }
                            else if (tools.TryGetValue(toolContent.Name, out var tool))
                            {
                                yield return new StreamingToolUseResponse { Name = toolContent.Name, Argument = toolContent.Arguments };
                                var result = await tool.InvokeAsync(toolContent.Arguments);
                                toolContent.Result = result;
                            }
                            else
                            {
                                toolContent.Result = FunctionResult.Failed("Tool not found");
                            }
                            yield return new StreamingToolResultResponse { Name = toolContent.Name, Result = toolContent.Result };
                        }
                    }
                    var cloneHistory = history.Clone();
                    cloneHistory.AddAssistantMessages(contents);

                    await foreach (var stream in StreamingChatCompletionAsync(cloneHistory, options))
                    {
                        yield return stream;
                    }
                }
            }
            else if (response is ContentStartEvent contentStart)
            {
                if (contentStart.ContentBlock is TextMessageContent textContent)
                {
                    contents.Add(new TextContentBlock { Text = textContent.Text });                }
                else if (contentStart.ContentBlock is ToolUseMessageContent toolContent)
                {
                    contents.Add(new ToolContentBlock { ID = toolContent.ID, Name = toolContent.Name });
                }
            }
            else if (response is ContentDeltaEvent contentDelta)
            {
                var content = contents[contentDelta.Index];
                if (content is TextContentBlock textContent)
                {
                    if (contentDelta.ContentBlock is TextDeltaMessageContent textDelta)
                    {
                        textContent.Text = textDelta.Text;
                    }
                    yield return new StreamingTextResponse { Text = textContent.Text };
                }
                else if (content is ToolContentBlock toolContent)
                {
                    if (contentDelta.ContentBlock is ToolUseDeltaMessageContent toolDelta)
                    {
                        toolContent.Arguments += toolDelta.PartialJson;
                    }
                    yield return new StreamingToolCallResponse { Name = toolContent.Name, Argument = toolContent.Arguments };
                }
            }
            else if (response is ContentStopEvent)
            {
                // nothing to do
            }
            else
            {
                yield return new StreamingErrorResponse { Message = "Unknown anthropic stream response message" };
            }
        }
    }

    private MessagesRequest BuildMessagesRequest(ChatHistory history, ChatOptions options)
    {
        var request = new MessagesRequest
        {
            Model = options.ModelId,
            MaxTokens = options.MaxTokens,
            System = options.System,
            Temperature = options.Temperature,
            TopK = options.TopK,
            TopP = options.TopP,
            StopSequences = options.StopSequences,
            Messages = []
        };

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
                var assistantContents = new List<MessageContent>();
                var userContents = new List<MessageContent>();
                foreach (var content in message.Contents)
                {
                    if (content is TextContentBlock text)
                    {
                        assistantContents.Add(new TextMessageContent { Text = text.Text ?? string.Empty });
                    }
                    else if (content is ToolContentBlock tool)
                    {
                        assistantContents.Add(new ToolUseMessageContent
                        {
                            ID = tool.ID,
                            Name = tool.Name,
                            Input = JsonSerializer.Deserialize<JsonObject>(tool.Arguments ?? string.Empty)
                        });
                        userContents.Add(new ToolResultMessageContent
                        {
                            IsError = tool.Result?.IsSuccess != true,
                            ToolUseID = tool.ID,
                            Content = [ new TextMessageContent { Text = JsonSerializer.Serialize(tool.Result) } ]
                        });
                    }
                }
                messages.Add(new Message { Role = "assistant", Content = assistantContents.ToArray() });

                if (userContents.Count > 0)
                {
                    messages.Add(new Message { Role = "user", Content = userContents.ToArray() });
                }
            }
        }
        request.Messages = messages.ToArray();

        if (options.Tools != null && options.Tools.Length > 0)
        {
            var tools = new List<Tool>();
            foreach (var tool in options.Tools)
            {
                var schema = tool.GetParametersJsonSchema();
                tools.Add(new Tool
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    InputSchema = new InputSchema
                    {
                        Properties = schema.Properties,
                        Required = schema.Required
                    }
                });
            }
            request.Tools = tools.ToArray();
        }

        return request;
    }
}
