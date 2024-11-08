using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using Raggle.Connector.Anthropic.ChatCompletion;
using Raggle.Connector.Anthropic.ChatCompletion.Models;
using Raggle.Connector.Anthropic.Configurations;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Raggle.Connector.Anthropic;

public class AnthropicChatCompletionService : IChatCompletionService
{
    private readonly AnthropicChatCompletionClient _client;

    public AnthropicChatCompletionService(AnthropicConfig config)
    {
        _client = new AnthropicChatCompletionClient(config);
    }

    public AnthropicChatCompletionService(string apiKey)
    {
        _client = new AnthropicChatCompletionClient(apiKey);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var anthropicModels = _client.GetChatCompletionModels();
        cancellationToken.ThrowIfCancellationRequested();

        var models = anthropicModels.Select(m => new ChatCompletionModel
        {
            Model = m.ModelId,
            CreatedAt = null,
            ModifiedAt = null,
            Owner = "Anthropic"
        });
        return Task.FromResult(models);
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse> ChatCompletionAsync(
        ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var anthropicRequest = ConvertToAnthropicRequest(request);
        var response = await _client.PostMessagesAsync(anthropicRequest, cancellationToken);

        var tools = request.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
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
                        Arguments = toolUseContent.Input?.ToString() ?? string.Empty
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

            var cloneHistory = request.Messages.Clone();
            cloneHistory.AddAssistantMessages(contents);
            request.Messages = cloneHistory;
            return await ChatCompletionAsync(request, cancellationToken);
        }
        else
        {
            if (request.Messages.TryGetLastAssistantMessage(out var lastAssistantMessage))
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
            var tokenUsage = new Abstractions.AI.TokenUsage
            {
                TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens,
                InputTokens = response.Usage.InputTokens,
                OutputTokens = response.Usage.OutputTokens
            };

            if (response.StopReason == StopReason.MaxTokens)
            {
                return new ChatCompletionResponse
                {
                    Completed = false,
                    Contents = contents.ToArray(),
                    TokenUsage = tokenUsage,
                    ErrorMessage = "You have reached the maximum tokens limit",
                };
            }
            else
            {
                return new ChatCompletionResponse
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
        ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var anthropicRequest = ConvertToAnthropicRequest(request);
        var tools = request.Tools?.ToDictionary(t => t.Name, t => t) ?? new Dictionary<string, FunctionTool>();
        var contents = new List<IContentBlock>();

        await foreach (var response in _client.PostStreamingMessagesAsync(anthropicRequest, cancellationToken))
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
                    var cloneHistory = request.Messages.Clone();
                    cloneHistory.AddAssistantMessages(contents);
                    request.Messages = cloneHistory;

                    await foreach (var stream in StreamingChatCompletionAsync(request, cancellationToken))
                    {
                        yield return stream;
                    }
                }
            }
            else if (response is ContentStartEvent contentStart)
            {
                if (contentStart.ContentBlock is TextMessageContent textContent)
                {
                    contents.Add(new TextContentBlock { Text = textContent.Text });
                }
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

    #region Private Methods

    private MessagesRequest ConvertToAnthropicRequest(ChatCompletionRequest request)
    {
        var anthropicRequest = new MessagesRequest
        {
            Model = request.Model,
            MaxTokens = request.MaxTokens ?? 2048,
            System = request.System,
            Temperature = request.Temperature,
            TopK = request.TopK,
            TopP = request.TopP,
            StopSequences = request.StopSequences,
            Messages = []
        };

        var messages = new List<Message>();
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
        anthropicRequest.Messages = messages.ToArray();

        if (request.Tools != null && request.Tools.Length > 0)
        {
            var tools = new List<Tool>();
            foreach (var tool in request.Tools)
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
            anthropicRequest.Tools = tools.ToArray();
        }

        return anthropicRequest;
    }

    #endregion
}
