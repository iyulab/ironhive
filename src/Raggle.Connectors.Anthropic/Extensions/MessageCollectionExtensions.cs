using System.Text.Json;
using Raggle.Connectors.Anthropic.ChatCompletion;
using AnthropicMessage = Raggle.Connectors.Anthropic.ChatCompletion.Message;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Connectors.Anthropic.ChatCompletion;

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

            if (message.GetType() == typeof(UserMessage))
            {
                var um = new AnthropicMessage
                {
                    Role = MessageRole.User,
                    Content = new List<MessageContent>()
                };
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        um.Content.Add(new TextMessageContent
                        {
                            Text = text.Text ?? string.Empty
                        });
                    }
                    else if (item is ImageContent image)
                    {
                        um.Content.Add(new ImageMessageContent
                        {
                            Source = new ImageSource
                            {
                                Data = image.Data!,
                                MediaType = "image/jpg"
                            }
                        });
                    }
                }
                _messages.Add(um);
            }
            else if (message.GetType() == typeof(AssistantMessage))
            {
                foreach (var content in message.Content.SplitContent())
                {
                    var am = new AnthropicMessage
                    {
                        Role = MessageRole.Assistant,
                        Content = new List<MessageContent>()
                    };
                    var um = new AnthropicMessage
                    {
                        Role = MessageRole.User,
                        Content = new List<MessageContent>()
                    };
                    foreach (var item in content)
                    {
                        if (item is TextContent text)
                        {
                            am.Content.Add(new TextMessageContent
                            {
                                Text = text.Text ?? string.Empty
                            });
                        }
                        else if (item is ToolContent tool)
                        {
                            am.Content.Add(new ToolUseMessageContent
                            {
                                ID = tool.Id,
                                Name = tool.Name,
                                Input = tool.Arguments
                            });
                            um.Content.Add(new ToolResultMessageContent
                            {
                                IsError = tool.Result?.IsSuccess != true,
                                ToolUseID = tool.Id,
                                Content = JsonSerializer.Serialize(tool.Result)
                            });
                        }
                    }
                    _messages.Add(am);
                    _messages.Add(um);
                }
            }
        }

        return _messages;
    }

    /// <summary>
    /// [Text, Tool...] 을 한쌍으로 Content를 분리합니다.
    /// 예) TextContent, ToolContent, TextContent 
    ///     => [TextContent, ToolContent] [TextContent]
    /// </summary>
    internal static IEnumerable<MessageContentCollection> SplitContent(this MessageContentCollection content)
    {
        var result = new List<MessageContentCollection>();
        var group = new MessageContentCollection();
        IMessageContent? previous = null;

        foreach (var item in content)
        {
            // 이전 아이템이 ToolContent이고 현재 아이템이 TextContent라면 새 그룹을 시작합니다.
            if (previous is ToolContent && item is TextContent)
            {
                result.Add(group);
                group = new MessageContentCollection();
            }

            group.Add(item);
            previous = item;
        }

        // 마지막 그룹이 있다면 추가합니다.
        if (group.Count != 0)
        {
            result.Add(group);
        }

        return result;
    }
}
