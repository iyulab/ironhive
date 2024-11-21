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
        var ollamaModels = await _client.GetChatModelsAsync(cancellationToken);
        return ollamaModels.Select(m => new ChatCompletionModel
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
        var ollamaRequest = ConvertToOllamaRequest(request);
        var res = await _client.PostChatAsync(ollamaRequest, cancellationToken);
        return new ChatCompletionResponse
        {
            Completed = true,
            Contents = 
            [
                new TextContentBlock { Text = res.Message?.Content }
            ]
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ollamaRequest = ConvertToOllamaRequest(request);
        await foreach (var res in _client.PostSteamingChatAsync(ollamaRequest, cancellationToken))
        {
            yield return new StreamingTextResponse
            {
                Text = res.Message.Content
            };
        }
    }

    #region Private Methods

    private static ChatRequest ConvertToOllamaRequest(ChatCompletionRequest request)
    {
        var messages = new List<ChatCompletion.Models.ChatMessage>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            messages.Add(new ChatCompletion.Models.ChatMessage 
            {
                Role = ChatRole.System,
                Content = request.System
            });
        }

        foreach (var message in request.Messages)
        {
            if (message.Contents == null || message.Contents.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var userMessage = new ChatCompletion.Models.ChatMessage { Role = ChatRole.User };
                foreach (var content in message.Contents)
                {
                    if (content is TextContentBlock text)
                    {
                        userMessage.Content ??= string.Empty;
                        userMessage.Content += text.Text;
                    }
                    else if (content is ImageContentBlock image)
                    {
                        userMessage.Images ??= new List<string>();
                        userMessage.Images.Add(image.Data);
                    }
                }
                messages.Add(userMessage);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var content in message.Contents)
                {
                    var assistantMessage = new ChatCompletion.Models.ChatMessage { Role = ChatRole.Assistant };
                    var toolMessages = new ChatCompletion.Models.ChatMessage { Role = ChatRole.Tool };
                    if (content is TextContentBlock text)
                    {
                        assistantMessage.Content ??= string.Empty;
                        assistantMessage.Content += text.Text;
                    }
                    else if (content is ToolContentBlock tool)
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

        if (request.Tools != null && request.Tools.Length > 0)
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
