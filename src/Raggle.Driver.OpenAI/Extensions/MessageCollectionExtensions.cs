using Raggle.Abstractions.Messages;
using Raggle.Driver.OpenAI.ChatCompletion;
using System.Text.Json;
using OpenAIMessage = Raggle.Driver.OpenAI.ChatCompletion.Message;

namespace Raggle.Driver.OpenAI.Extensions;

internal static class MessageCollectionExtensions
{
    internal static IEnumerable<OpenAIMessage> ToOpenAI(this MessageCollection messages, string? system = null)
    {
        var _messages = new List<OpenAIMessage>();
        if (!string.IsNullOrWhiteSpace(system))
        {
            _messages.Add(new DeveloperMessage { Content = system });
        }

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

        return _messages;
    }
}
