using Raggle.Abstractions.Messages;
using Raggle.Driver.OpenAI.ChatCompletion.Models;
using OpenAIChatCompletionRequest = Raggle.Driver.OpenAI.ChatCompletion.Models.ChatCompletionRequest;
using OpenAIMessage = Raggle.Driver.OpenAI.ChatCompletion.Models.Message;
using System.Text.Json;

namespace Raggle.Abstractions.AI;

internal static class ChatCompletionRequestExtensions
{
    internal static OpenAIChatCompletionRequest ToOpenAI(this ChatCompletionRequest request)
    {
        var isReason = request.Model.Contains("o1") || request.Model.Contains("o3");
        var _request = new OpenAIChatCompletionRequest
        {
            Model = request.Model,
            MaxCompletionTokens = request.MaxTokens,
            Temperature = isReason ? null : request.Temperature,
            TopP = isReason ? null : request.TopP,
            Stop = request.StopSequences,
            Messages = [],
        };

        var _messages = new List<OpenAIMessage>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            _messages.Add(new DeveloperMessage { Content = request.System });
        }

        foreach (var message in request.Messages)
        {
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                var content = new List<MessageContent>();
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        content.Add(new TextMessageContent { Text = text.Text ?? string.Empty });
                    }
                    else if (item is ImageContent image)
                    {
                        content.Add(new ImageMessageContent { ImageURL = new ImageURL { URL = image.Data ?? string.Empty } });
                    }
                }
                _messages.Add(new UserMessage { Content = content });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        _messages.Add(new AssistantMessage { Content = text.Text });
                    }
                    else if (item is ToolContent tool)
                    {
                        _messages.Add(new AssistantMessage
                        {
                            ToolCalls = [
                                new ToolCall
                                {
                                    Index = tool.Index,
                                    ID = tool.Id,
                                    Function = new FunctionCall
                                    {
                                        Name = tool.Name,
                                        Arguments = tool.Arguments?.ToString()
                                    }
                                }
                            ]
                        });
                        if (tool.Result == null) continue;

                        _messages.Add(new ToolMessage
                        {
                            ID = tool.Id ?? string.Empty,
                            Content = JsonSerializer.Serialize(tool.Result)
                        });
                    }
                }
            }
        }
        _request.Messages = _messages.ToArray();

        if (request.Tools != null && request.Tools.Count > 0)
        {
            _request.Tools = request.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.ToJsonSchema()
                }
            }).ToArray();
        }

        return _request;
    }
}
