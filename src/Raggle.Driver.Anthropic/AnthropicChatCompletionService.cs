using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using Raggle.Driver.Anthropic.ChatCompletion;
using Raggle.Driver.Anthropic.ChatCompletion.Models;
using Raggle.Driver.Anthropic.Configurations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Raggle.Driver.Anthropic;

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
        var _request = ConvertToAnthropicRequest(request);
        var response = await _client.PostMessagesAsync(_request, cancellationToken);

        var content = request.Messages.Last().Role == MessageRole.Assistant
            ? request.Messages.Last().Content
            : new MessageContentCollection();

        if (response.StopReason == StopReason.ToolUse)
        {
            foreach(var item in response.Content)
            {
                if (item is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else if (item is ToolUseMessageContent toolUse)
                {
                    var id = toolUse.ID;
                    var name = toolUse.Name;
                    var args = JsonSerializer.Serialize(toolUse.Input);
                    var result = string.IsNullOrWhiteSpace(name)
                        ? FunctionResult.Failed("Tool name is missing")
                        : request.Tools != null && request.Tools.TryGetValue(name, out var tool)
                        ? await tool.InvokeAsync(args)
                        : FunctionResult.Failed($"Tool [{name}] not found");

                    content.AddTool(id, name, args, result);
                }
            }

            request.Messages.AddAssistantMessage(content);
            return await ChatCompletionAsync(request, cancellationToken);
        }
        else
        {
            foreach(var item in response.Content)
            {
                if (item is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else
                {
                    Debug.WriteLine($"Unexpected message content type: {item.GetType()}");
                    //throw new InvalidOperationException("Unexpected message content type");
                }
            }

            var result = new ChatCompletionResponse
            {
                EndReason = response.StopReason switch
                {
                    StopReason.EndTurn => ChatCompletionEndReason.EndTurn,
                    StopReason.MaxTokens => ChatCompletionEndReason.MaxTokens,
                    StopReason.StopSequence => ChatCompletionEndReason.StopSequence,
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
                    TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens,
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens
                },
            };

            return result;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = ConvertToAnthropicRequest(request);

        var content = new MessageContentCollection();
        await foreach (var response in _client.PostStreamingMessagesAsync(_request, cancellationToken))
        {
            if (response is MessageDeltaEvent messageDelta)
            {
                var reason = messageDelta.Delta?.StopReason;
                if (reason == StopReason.ToolUse)
                {
                    foreach (var item in content)
                    {
                        if (item is ToolContent toolContent)
                        {
                            toolContent.Result = string.IsNullOrWhiteSpace(toolContent.Name)
                                    ? FunctionResult.Failed("Tool name is missing")
                                    : request.Tools != null && request.Tools.TryGetValue(toolContent.Name, out var tool)
                                    ? await tool.InvokeAsync(toolContent.Arguments)
                                    : FunctionResult.Failed($"Tool [{toolContent.Name}] not found");
                            yield return new ChatCompletionStreamingResponse 
                            { 
                                Content = toolContent
                            };
                        }
                    }

                    request.Messages.AddAssistantMessage(content);
                    await foreach (var stream in StreamingChatCompletionAsync(request, cancellationToken))
                    {
                        yield return stream;
                    }
                }
                else
                {
                    yield return new ChatCompletionStreamingResponse
                    {
                        Model = request.Model,
                        EndReason = reason switch
                        {
                            StopReason.EndTurn => ChatCompletionEndReason.EndTurn,
                            StopReason.StopSequence => ChatCompletionEndReason.StopSequence,
                            StopReason.MaxTokens => ChatCompletionEndReason.MaxTokens,
                            _ => null
                        },
                        TokenUsage = new Abstractions.AI.TokenUsage
                        {
                            TotalTokens = messageDelta.Usage?.InputTokens + messageDelta.Usage?.OutputTokens,
                            InputTokens = messageDelta.Usage?.InputTokens,
                            OutputTokens = messageDelta.Usage?.OutputTokens
                        }
                    };
                }
            }
            else if (response is ContentStartEvent contentStart)
            {
                if (contentStart.ContentBlock is TextMessageContent text)
                {
                    content.AddText(text.Text);
                }
                else if (contentStart.ContentBlock is ToolUseMessageContent tool)
                {
                    content.AddTool(tool.ID, tool.Name, null, null);
                }
            }
            else if (response is ContentDeltaEvent contentDelta)
            {
                var item = content.ElementAt(contentDelta.Index);
                if (item is TextContent textContent)
                {
                    if (contentDelta.ContentBlock is TextDeltaMessageContent textDelta)
                    {
                        textContent.Text = textDelta.Text;
                    }
                    yield return new ChatCompletionStreamingResponse { Content = textContent };
                }
                else if (item is ToolContent toolContent)
                {
                    if (contentDelta.ContentBlock is ToolUseDeltaMessageContent toolDelta)
                    {
                        toolContent.Arguments += toolDelta.PartialJson;
                    }
                    yield return new ChatCompletionStreamingResponse { Content = toolContent };
                }
            }
            else if (response is ErrorEvent error)
            {
                throw new InvalidOperationException(error.Error.ToString());
            }
            else if (response is PingEvent)
            {
                // nothing to do
            }
            else if (response is MessageStartEvent)
            {
                // nothing to do
            }
            else if (response is MessageStopEvent)
            {
                // nothing to do
            }
            else if (response is ContentStopEvent)
            {
                // nothing to do
            }
            else
            {
                // unexpected event nothing to do
                Debug.WriteLine($"Unexpected event: {response.GetType()}");
            }
        }
    }

    #region Private Methods

    private MessagesRequest ConvertToAnthropicRequest(ChatCompletionRequest request)
    {
        var _request = new MessagesRequest
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

        var _messages = new List<ChatCompletion.Models.Message>();
        foreach (var message in request.Messages)
        {
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var _content = new List<MessageContent>();
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        _content.Add(new TextMessageContent 
                        { 
                            Text = text.Text ?? string.Empty 
                        });
                    }
                    else if (item is ImageContent image)
                    {
                        _content.Add(new ImageMessageContent 
                        { 
                            Source = new ImageSource 
                            { 
                                Data = image.Data!, 
                                MediaType = "image/jpg" 
                            } 
                        });
                    }
                }
                _messages.Add(new ChatCompletion.Models.Message 
                {
                    Role = "user", 
                    Content = _content.ToArray() 
                });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        if (_messages.Last().Role == "assistant")
                        {
                            _messages.Last().Content.Add(new TextMessageContent
                            {
                                Text = text.Text ?? string.Empty
                            });
                        }
                        else
                        {
                            _messages.Add(new ChatCompletion.Models.Message
                            {
                                Role = "assistant",
                                Content =
                                [
                                    new TextMessageContent
                                    {
                                        Text = text.Text ?? string.Empty
                                    }
                                ]
                            });
                        }
                    }
                    else if (item is ToolContent tool)
                    {
                        if (_messages.Last().Role == "assistant")
                        {
                            _messages.Last().Content.Add(new ToolUseMessageContent
                            {
                                ID = tool.Id,
                                Name = tool.Name,
                                Input = JsonSerializer.Deserialize<JsonObject>(tool.Arguments ?? string.Empty)
                            });
                        }
                        else
                        {
                            _messages.Add(new ChatCompletion.Models.Message
                            {
                                Role = "assistant",
                                Content =
                                [
                                    new ToolUseMessageContent
                                    {
                                        ID = tool.Id,
                                        Name = tool.Name,
                                        Input = JsonSerializer.Deserialize<JsonObject>(tool.Arguments ?? string.Empty)
                                    }
                                ]
                            });
                        }
                        _messages.Add(new ChatCompletion.Models.Message
                        {
                            Role = "user",
                            Content =
                            [
                                new ToolResultMessageContent
                                {
                                    IsError = tool.Result?.IsSuccess != true,
                                    ToolUseID = tool.Id,
                                    Content =
                                    [
                                        new TextMessageContent
                                        {
                                            Text = JsonSerializer.Serialize(tool.Result)
                                        }
                                    ]
                                }
                            ]
                        });
                    }
                }
            }
        }
        _request.Messages = _messages.ToArray();

        if (request.Tools != null && request.Tools.Count > 0)
        {
            var _tools = new List<Tool>();
            foreach (var tool in request.Tools)
            {
                var schema = tool.GetParametersJsonSchema();
                _tools.Add(new Tool
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
            _request.Tools = _tools.ToArray();
        }

        return _request;
    }

    #endregion
}
