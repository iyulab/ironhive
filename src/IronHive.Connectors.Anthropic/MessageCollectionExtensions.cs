using IronHive.Connectors.Anthropic.ChatCompletion;
using AnthropicMessage = IronHive.Connectors.Anthropic.ChatCompletion.Message;
using MessageRole = IronHive.Abstractions.Messages.MessageRole;
using AnthropicMessageRole = IronHive.Connectors.Anthropic.ChatCompletion.MessageRole;
using System.Text.Json;
using IronHive.Abstractions.Messages;

namespace IronHive.Connectors.Anthropic;

internal static class MessageCollectionExtensions
{
    internal static IEnumerable<AnthropicMessage> ToAnthropic(this MessageCollection messages)
    {
        var _messages = new List<AnthropicMessage>();

        foreach (var message in messages)
        {
            // 건너뛰기
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                // 사용자 메시지
                var um = new AnthropicMessage(AnthropicMessageRole.User);
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
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                _messages.Add(um);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                // AI 메시지
                foreach (var (type, group) in message.Content.Split())
                {
                    if (type == typeof(TextContent))
                    {
                        // 텍스트 메시지
                        var am = new AnthropicMessage(AnthropicMessageRole.Assistant);
                        foreach (var item in group.Cast<TextContent>())
                        {
                            am.Content.Add(new TextMessageContent
                            {
                                Text = item.Value ?? string.Empty
                            });
                        }
                        _messages.Add(am);
                    }
                    else if (type == typeof(ToolContent))
                    {
                        // 도구 메시지
                        var am = new AnthropicMessage(AnthropicMessageRole.Assistant);
                        var um = new AnthropicMessage(AnthropicMessageRole.User);
                        foreach (var tool in group.Cast<ToolContent>())
                        {
                            am.Content.Add(new ToolUseMessageContent
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Input = !string.IsNullOrWhiteSpace(tool.Arguments)
                                    ? JsonSerializer.Deserialize<IDictionary<string, object>>(tool.Arguments)
                                    : new object()
                            });
                            um.Content.Add(new ToolResultMessageContent
                            {
                                IsError = ToolStatus.Failed == tool.Status,
                                ToolUseId = tool.Id,
                                Content = tool.Result ?? string.Empty
                            });
                        }
                        _messages.Add(am);
                        _messages.Add(um);
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return _messages;
    }
}
