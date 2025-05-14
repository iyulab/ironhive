using IronHive.Connectors.OpenAI.ChatCompletion;
using OpenAIMessage = IronHive.Connectors.OpenAI.ChatCompletion.Message;
using UserMessage = IronHive.Abstractions.Messages.UserMessage;
using OpenAIUserMessage = IronHive.Connectors.OpenAI.ChatCompletion.UserMessage;
using AssistantMessage = IronHive.Abstractions.Messages.AssistantMessage;
using OpenAIAssistantMessage = IronHive.Connectors.OpenAI.ChatCompletion.AssistantMessage;
using IronHive.Abstractions.Messages;

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
            if (message is UserMessage user)
            {
                // 사용자 메시지
                var um = new OpenAIUserMessage();
                foreach (var item in user.Content)
                {
                    if (item is UserTextContent text)
                    {
                        // 텍스트 메시지
                        um.Content.Add(new TextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    else if (item is UserImageContent image)
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
            else if (message is AssistantMessage assistant)
            {
                // AI 메시지
                foreach (var group in assistant.Content.Split())
                {
                    var am = new OpenAIAssistantMessage();
                    var tms = new List<ToolMessage>();
                    foreach (var content in group)
                    {
                        if (content is AssistantThinkingContent thinking)
                        {
                            // 추론 컨텐츠 건너뛰기
                        }
                        else if (content is AssistantTextContent text)
                        {
                            // 텍스트 메시지 (어시스턴트 메시지)
                            am.Content = text.Value ?? string.Empty;
                        }
                        else if (content is AssistantToolContent tool)
                        {
                            // 도구 메시지
                            am.ToolCalls ??= new List<ToolCall>();
                            am.ToolCalls.Add(new ToolCall
                            {
                                Index = tool.Index,
                                Id = tool.Id,
                                Function = new FunctionCall
                                {
                                    Name = tool.Name,
                                    Arguments = tool.Arguments
                                }
                            });

                            // 도구 결과 메시지
                            tms.Add(new ToolMessage
                            {
                                ID = tool.Id ?? string.Empty,
                                Content = tool.Result ?? string.Empty,
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    _messages.Add(am);
                    _messages.AddRange(tms);
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
