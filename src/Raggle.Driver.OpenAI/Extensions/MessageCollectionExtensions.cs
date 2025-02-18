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
                var um = new UserMessage
                { 
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
                            ImageURL = new ImageURL 
                            { 
                                URL = image.Data ?? string.Empty,
                            } 
                        });
                    }
                }
                _messages.Add(um);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                foreach (var content in message.Content.SplitContent())
                {
                    var am = new AssistantMessage();
                    var tms = new List<ToolMessage>();
                    foreach (var item in content)
                    {
                        if (item is TextContent text)
                        {
                            am.Content = text.Text ?? string.Empty;
                        }
                        else if (item is ToolContent tool)
                        {
                            am.ToolCalls ??= new List<ToolCall>();
                            am.ToolCalls.Add(new ToolCall
                            {
                                Index = tool.Index,
                                ID = tool.Id,
                                Function = new FunctionCall
                                {
                                    Name = tool.Name,
                                    Arguments = tool.Arguments?.ToString()
                                }
                            });

                            tms.Add(new ToolMessage
                            {
                                ID = tool.Id ?? string.Empty,
                                Content = JsonSerializer.Serialize(tool.Result)
                            });
                        }
                    }

                    _messages.Add(am);
                    if (tms.Count > 0)
                        _messages.AddRange(tms);
                }
            }
        }

        return _messages;
    }

    /// <summary>
    /// 동일한 종류의 content끼리 묶습니다.
    /// 예) ToolContent, ToolContent, ToolContent, TextContent, TextContent, ToolContent, ToolContent
    ///     => [ToolContent, ToolContent, ToolContent], [TextContent, TextContent], [ToolContent, ToolContent]
    /// </summary>
    internal static IEnumerable<MessageContentCollection> SplitContent(this MessageContentCollection content)
    {
        var result = new List<MessageContentCollection>();
        var group = new MessageContentCollection();
        Type? currentType = null;

        foreach (var item in content)
        {
            // 그룹이 비어있으면 현재 아이템 타입을 기준으로 그룹 시작
            if (group.Count == 0)
            {
                currentType = item.GetType();
            }
            // 현재 아이템 타입과 이전 그룹의 타입이 다르면 그룹 분리
            else if (item.GetType() != currentType)
            {
                result.Add(group);
                group = new MessageContentCollection();
                currentType = item.GetType();
            }

            group.Add(item);
        }

        // 마지막 그룹이 있다면 추가합니다.
        if (group.Count > 0)
        {
            result.Add(group);
        }

        return result;
    }
}
