using Raggle.Abstractions.Messages;
using Raggle.Driver.Ollama.Base;
using Raggle.Driver.Ollama.ChatCompletion.Models;
using System.Text.Json;

namespace Raggle.Abstractions.AI;

internal static class ChatCompletionRequestExtensions
{
    internal static ChatRequest ToOllama(this ChatCompletionRequest request)
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
                var schema = tool.ToJsonSchema();
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
}
