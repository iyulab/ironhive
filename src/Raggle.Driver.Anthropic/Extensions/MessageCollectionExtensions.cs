using Raggle.Abstractions.Messages;
using System.Text.Json;
using Raggle.Driver.Anthropic.ChatCompletion;
using MessageRole = Raggle.Abstractions.Messages.MessageRole;
using AnthropicMessage = Raggle.Driver.Anthropic.ChatCompletion.Message;
using AnthropicMessageRole = Raggle.Driver.Anthropic.ChatCompletion.MessageRole;

namespace Raggle.Abstractions.AI;

internal static class MessageCollectionExtensions
{
    internal static IEnumerable<AnthropicMessage> ToAnthropic(this MessageCollection messages)
    {
        var _messages = new List<AnthropicMessage>();
        foreach (var message in messages)
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
                        content.Add(new TextMessageContent
                        {
                            Text = text.Text ?? string.Empty
                        });
                    }
                    else if (item is ImageContent image)
                    {
                        content.Add(new ImageMessageContent
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
                    Role = AnthropicMessageRole.User,
                    Content = content.ToArray()
                });
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        if (_messages.Last().Role == AnthropicMessageRole.Assistant)
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
                                Role = AnthropicMessageRole.Assistant,
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
                        if (_messages.Last().Role == AnthropicMessageRole.Assistant)
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
                                Role = AnthropicMessageRole.Assistant,
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
                            Role = AnthropicMessageRole.User,
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

        return _messages;
    }
}
