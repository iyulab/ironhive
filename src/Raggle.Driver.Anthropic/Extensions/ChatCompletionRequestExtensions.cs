using Raggle.Abstractions.Messages;
using Raggle.Driver.Anthropic.ChatCompletion.Models;
using AnthropicMessage = Raggle.Driver.Anthropic.ChatCompletion.Models.Message;
using System.Text.Json;

namespace Raggle.Abstractions.AI;

internal static class ChatCompletionRequestExtensions
{
    internal static MessagesRequest ToAnthropic(this ChatCompletionRequest request)
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

        var _messages = new List<AnthropicMessage>();
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
                _messages.Add(new AnthropicMessage
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
                            _messages.Add(new AnthropicMessage
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
                                Input = tool.Arguments
                            });
                        }
                        else
                        {
                            _messages.Add(new AnthropicMessage
                            {
                                Role = "assistant",
                                Content =
                                [
                                    new ToolUseMessageContent
                                    {
                                        ID = tool.Id,
                                        Name = tool.Name,
                                        Input = tool.Arguments
                                    }
                                ]
                            });
                        }

                        _messages.Add(new AnthropicMessage
                        {
                            Role = "user",
                            Content =
                            [
                                new ToolResultMessageContent
                                {
                                    IsError = tool.Result?.IsSuccess != true,
                                    ToolUseID = tool.Id,
                                    Content = JsonSerializer.Serialize(tool.Result)
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
            _request.Tools = request.Tools.Select(t => new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.ToJsonSchema()
            }).ToArray();
        }

        return _request;
    }
}
