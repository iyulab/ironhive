using System.Text.Json;
using IronHive.Abstractions.Messages;
using IronHive.Connectors.Anthropic.ChatCompletion;
using AnthropicMessage = IronHive.Connectors.Anthropic.ChatCompletion.Message;

namespace IronHive.Connectors.Anthropic;

internal static class MessageCollectionExtensions
{
    internal static IEnumerable<AnthropicMessage> ToAnthropic(this MessageCollection messages, bool withThingking)
    {
        var _messages = new List<AnthropicMessage>();

        foreach (var message in messages)
        {
            // 사용자 메시지
            if (message is UserMessage user)
            {
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
                    else if (item is UserFileContent file)
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
            // AI 메시지
            else if (message is AssistantMessage assistant)
            {
                var groups = assistant.Content.SplitByTool();
                
                foreach (var group in groups)
                {
                    var am = new AnthropicMessage(MessageRole.Assistant);
                    var um = new AnthropicMessage(MessageRole.User);

                    // 추론 모델은 툴 사용 시나리오에서
                    // ToolUse 이전에 반드시 추론 메시지가 존재해야 합니다.
                    var isLastMessage = message == messages.Last();
                    var isFirstGroup = group == groups.First();
                    if (isLastMessage && isFirstGroup && withThingking)
                    {
                        foreach (var thinking in group.OfType<AssistantThinkingContent>())
                        {
                            am.Content.Add(thinking.Mode == ThinkingMode.Secure
                                ? new RedactedThinkingMessageContent
                                {
                                    Data = thinking.Value ?? string.Empty,
                                }
                                : new ThinkingMessageContent
                                {
                                    Signature = thinking.Id ?? string.Empty,
                                    Thinking = thinking.Value ?? string.Empty
                                });
                        }
                    }

                    foreach (var content in group)
                    {
                        // 추론 메시지
                        if (content is AssistantThinkingContent)
                        {
                            continue;
                        }
                        // 텍스트 메시지
                        else if (content is AssistantTextContent text)
                        {
                            am.Content.Add(new TextMessageContent
                            {
                                Text = text.Value ?? string.Empty
                            });
                        }
                        // 도구 메시지
                        else if (content is AssistantToolContent tool)
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
                                ToolUseId = tool.Id,
                                IsError = !tool.Result?.IsSuccess ?? false,
                                Content = tool.Result?.Data ?? string.Empty,
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    _messages.Add(am);
                    if (um.Content.Count > 0)
                        _messages.Add(um);
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
