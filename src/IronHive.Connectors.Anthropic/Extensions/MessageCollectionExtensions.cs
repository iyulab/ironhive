using IronHive.Connectors.Anthropic.ChatCompletion;
using AnthropicMessage = IronHive.Connectors.Anthropic.ChatCompletion.Message;
using IronHive.Abstractions.ChatCompletion.Messages;
using MessageRole = IronHive.Abstractions.ChatCompletion.Messages.MessageRole;
using AnthropicMessageRole = IronHive.Connectors.Anthropic.ChatCompletion.MessageRole;
using IronHive.Abstractions.ChatCompletion.Tools;
using System.Text.Json;

namespace IronHive.Abstractions.AI;

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
                var um = new AnthropicMessage
                {
                    Role = AnthropicMessageRole.User,
                    Content = new List<MessageContent>()
                };
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        um.Content.Add(new TextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    else if (item is ImageContent image)
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                _messages.Add(um);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var content in message.Content.SplitContent())
                {
                    var am = new AnthropicMessage
                    {
                        Role = AnthropicMessageRole.Assistant,
                        Content = new List<MessageContent>()
                    };
                    var um = new AnthropicMessage
                    {
                        Role = AnthropicMessageRole.User,
                        Content = new List<MessageContent>()
                    };
                    foreach (var item in content)
                    {
                        if (item is TextContent text)
                        {
                            am.Content.Add(new TextMessageContent
                            {
                                Text = text.Value ?? string.Empty
                            });
                        }
                        else if (item is ToolContent tool)
                        {
                            am.Content.Add(new ToolUseMessageContent
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Input = !string.IsNullOrWhiteSpace(tool.Arguments)
                                    ? JsonSerializer.Deserialize<IDictionary<string, object>>(tool.Arguments)
                                    : null
                            });

                            um.Content.Add(new ToolResultMessageContent
                            {
                                IsError = ToolStatus.Failed == tool.Status,
                                ToolUseId = tool.Id,
                                Content = tool.Result ?? string.Empty
                            });
                        }
                    }

                    _messages.Add(am);
                    if (um.Content.Count != 0)
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
