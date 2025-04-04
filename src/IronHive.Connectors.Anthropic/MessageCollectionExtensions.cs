using IronHive.Connectors.Anthropic.ChatCompletion;
using AnthropicMessage = IronHive.Connectors.Anthropic.ChatCompletion.Message;
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
            if (message is UserMessage user)
            {
                // 사용자 메시지
                var um = new AnthropicMessage(MessageRole.User);
                foreach (var item in user.Content)
                {
                    if (item is UserTextContent text)
                    {
                        um.Content.Add(new TextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    else if (item is UserImageContent image)
                    {
                        throw new NotSupportedException("not supported image yet");
                    }
                    else
                    {
                        throw new NotSupportedException($"not supported type {item.GetType()}");
                    }
                }

                _messages.Add(um);
            }
            else if (message is AssistantMessage assistant)
            {
                // AI 메시지
                foreach (var (type, group) in assistant.Content.Split())
                {
                    if (type == typeof(AssistantTextContent))
                    {
                        // 텍스트 메시지 (어시스턴트 메시지)
                        var am = new AnthropicMessage(MessageRole.Assistant);
                        foreach (var item in group.Cast<AssistantTextContent>())
                        {
                            am.Content.Add(new TextMessageContent
                            {
                                Text = item.Value ?? string.Empty
                            });
                        }
                        _messages.Add(am);
                    }
                    else if (type == typeof(AssistantToolContent))
                    {
                        // 도구 메시지
                        var am = new AnthropicMessage(MessageRole.Assistant);
                        var um = new AnthropicMessage(MessageRole.User);
                        foreach (var tool in group.Cast<AssistantToolContent>())
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
                                IsError = (tool.Status == ToolStatus.Failed),
                                ToolUseId = tool.Id,
                                Content = tool.Result ?? string.Empty,
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
