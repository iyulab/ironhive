using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Driver.Ollama.ChatCompletion;
using Raggle.Driver.Ollama.ChatCompletion.Models;
using Raggle.Driver.Ollama.Configurations;
using System.Text.Json;
using Raggle.Driver.Ollama.Base;
using System.Runtime.CompilerServices;

namespace Raggle.Driver.Ollama;

public class OllamaChatCompletionService : IChatCompletionService
{
    private readonly OllamaChatCompletionClient _client;

    public OllamaChatCompletionService(OllamaConfig? config = null)
    {
        _client = new OllamaChatCompletionClient(config);
    }

    public OllamaChatCompletionService(string endPoint)
    {
        _client = new OllamaChatCompletionClient(endPoint);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetChatModelsAsync(cancellationToken);
        return models.Select(m => new ChatCompletionModel
        {
            Model = m.Name,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
            Owner = null
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse> ChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOllamaRequest(request);
        var response = await _client.PostChatAsync(_request, cancellationToken);
        var content = new MessageContentCollection();
        content.AddText(response.Message?.Content);
        return new ChatCompletionResponse
        {
            Model = response.Model,
            EndReason = response.DoneReason switch
            {
                DoneReason.Stop => ChatCompletionEndReason.EndTurn,
                _ => null
            },
            Message = new Message
            {
                Role = MessageRole.Assistant,
                Content = content,
                TimeStamp = DateTime.UtcNow,
            },
            TokenUsage = null,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOllamaRequest(request);
        await foreach (var res in _client.PostSteamingChatAsync(_request, cancellationToken))
        {
            yield return new ChatCompletionStreamingResponse
            {
                Content = new TextContent
                {
                    Text = res.Message?.Content
                },
            };
        }
    }

    #region Private Methods

    private static ChatRequest ConvertToOllamaRequest(ChatCompletionRequest request)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            messages.Add(new ChatMessage 
            {
                Role = ChatRole.System,
                Content = request.System
            });
        }

        foreach (var message in request.Messages)
        {
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var userMessage = new ChatMessage
                { 
                    Role = ChatRole.User
                };
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        userMessage.Content ??= string.Empty;
                        userMessage.Content += text.Text;
                    }
                    else if (item is ImageContent image)
                    {
                        userMessage.Images ??= new List<string>();
                        userMessage.Images.Add(image.Data);
                    }
                }
                messages.Add(userMessage);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var item in message.Content)
                {
                    var assistantMessage = new ChatMessage 
                    { 
                        Role = ChatRole.Assistant
                    };
                    var toolMessages = new ChatMessage 
                    { 
                        Role = ChatRole.Tool
                    };
                    if (item is TextContent text)
                    {
                        assistantMessage.Content ??= string.Empty;
                        assistantMessage.Content += text.Text;
                    }
                    else if (item is ToolContent tool)
                    {
                        assistantMessage.ToolCalls ??= new List<ToolCall>();
                        assistantMessage.ToolCalls.Add(new ToolCall
                        {
                            Function = new FunctionCall
                            {
                                Name = tool.Name,
                                //!!! Argument = tool.Arguments // This is not implemented in the Ollama API
                                //Arguments = tool.Arguments
                            }
                        });
                        toolMessages.Content ??= string.Empty;
                        toolMessages.Content += JsonSerializer.Serialize(tool.Result);
                    }

                    messages.Add(assistantMessage);
                    if (toolMessages.Content != null)
                        messages.Add(toolMessages);
                }
            }
        }

        var ollamaRequest = new ChatRequest
        {
            Model = request.Model,
            Messages = messages.ToArray(),
            Options = new ModelOptions
            {
                NumPredict = request.MaxTokens,
                Temperature = request.Temperature,
                TopP = request.TopP,
                TopK = request.TopK,
                Stop = request.StopSequences != null ? string.Join(" ", request.StopSequences) : null,
            },
        };

        if (request.Tools != null && request.Tools.Count > 0)
        {
            var tools = new List<Tool>();
            foreach (var tool in request.Tools)
            {
                var schema = tool.GetParametersJsonSchema();
                tools.Add(new Tool
                {
                    Function = new FunctionTool
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = new ParametersSchema
                        {
                            Properties = schema.Properties,
                            Required = schema.Required,
                        }
                    }
                });
            }
            ollamaRequest.Tools = tools.ToArray();
        }

        return ollamaRequest;
    }

    #endregion
}
