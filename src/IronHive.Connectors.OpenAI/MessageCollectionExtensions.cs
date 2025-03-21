using IronHive.Connectors.OpenAI.ChatCompletion;
using OpenAIMessage = IronHive.Connectors.OpenAI.ChatCompletion.Message;
using OpenAIUserMessage = IronHive.Connectors.OpenAI.ChatCompletion.UserMessage;
using OpenAIAssistantMessage = IronHive.Connectors.OpenAI.ChatCompletion.AssistantMessage;
using IronHive.Abstractions.ChatCompletion.Messages;

namespace IronHive.Connectors.OpenAI;

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
            // 건너뛰기
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.Role == MessageRole.User)
            {
                // 사용자 메시지
                var um = new OpenAIUserMessage();
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        // 텍스트 메시지
                        um.Content.Add(new TextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    else if (item is ImageContent image)
                    {
                        // 이미지 메시지
                        um.Content.Add(new ImageMessageContent
                        {
                            ImageURL = new ImageURL
                            {
                                URL = image.Data ?? string.Empty,
                            }
                        });
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
                        var am = new OpenAIAssistantMessage();
                        foreach (var item in group.Cast<TextContent>())
                        {
                            am.Content ??= string.Empty;
                            am.Content += item.Value ?? string.Empty;
                        }
                        _messages.Add(am);
                    }
                    else if (type == typeof(ToolContent))
                    {
                        // 도구 메시지
                        var am = new OpenAIAssistantMessage();
                        var tms = new List<ToolMessage>();
                        foreach (var item in group.Cast<ToolContent>())
                        {
                            am.ToolCalls ??= new List<ToolCall>();
                            am.ToolCalls.Add(new ToolCall
                            {
                                Index = item.Index,
                                Id = item.Id,
                                Function = new FunctionCall
                                {
                                    Name = item.Name,
                                    Arguments = item.Arguments
                                }
                            });
                            tms.Add(new ToolMessage
                            {
                                ID = item.Id ?? string.Empty,
                                Content = item.Result ?? string.Empty,
                            });
                        }
                        _messages.Add(am);
                        _messages.AddRange(tms);
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
