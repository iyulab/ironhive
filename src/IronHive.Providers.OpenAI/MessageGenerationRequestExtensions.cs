using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Providers.OpenAI.ChatCompletion;
using UserMessage = IronHive.Abstractions.Message.Roles.UserMessage;
using AssistantMessage = IronHive.Abstractions.Message.Roles.AssistantMessage;
using OpenAIMessage = IronHive.Providers.OpenAI.ChatCompletion.Message;
using OpenAIUserMessage = IronHive.Providers.OpenAI.ChatCompletion.UserMessage;
using OpenAIAssistantMessage = IronHive.Providers.OpenAI.ChatCompletion.AssistantMessage;
using TextMessageContent = IronHive.Abstractions.Message.Content.TextMessageContent;
using OpenAITextMessageContent = IronHive.Providers.OpenAI.ChatCompletion.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Message.Content.ImageMessageContent;
using OpenAIImageMessageContent = IronHive.Providers.OpenAI.ChatCompletion.ImageMessageContent;

namespace IronHive.Providers.OpenAI;

internal static class MessageGenerationRequestExtensions
{
    internal static ChatCompletionRequest ToOpenAI(this MessageGenerationRequest request)
    {
        var isReasoning = IsReasoningModel(request);

        var messages = new List<OpenAIMessage>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            if (isReasoning)
                messages.Add(new DeveloperMessage { Content = request.System });
            else
                messages.Add(new SystemMessage { Content = request.System });
        }

        foreach (var message in request.Messages)
        {
            if (message is UserMessage user)
            {
                // 사용자 메시지
                var um = new OpenAIUserMessage();
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        um.Content.Add(new OpenAITextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        um.Content.Add(new OpenAITextMessageContent
                        {
                            Text = $"Document Content:\n{document.Data}"
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new OpenAIImageMessageContent
                        {
                            ImageUrl = new ImageUrl
                            {
                                Url = image.Base64
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                messages.Add(um);
            }
            else if (message is AssistantMessage assistant)
            {
                // AI 메시지
                foreach (var group in assistant.SplitContentByTool())
                {
                    var am = new OpenAIAssistantMessage();
                    var tml = new List<ToolMessage>();
                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent thinking)
                        {
                            // 추론 컨텐츠 건너뛰기
                        }
                        else if (content is TextMessageContent text)
                        {
                            // 텍스트 메시지 (어시스턴트 메시지)
                            am.Content ??= string.Empty;
                            am.Content += text.Value;
                        }
                        else if (content is ToolMessageContent tool)
                        {
                            // 도구 메시지
                            am.ToolCalls ??= [];
                            am.ToolCalls.Add(new ToolCall
                            {
                                //Index = tool.Index,
                                Id = tool.Id,
                                Function = new FunctionCall
                                {
                                    Name = tool.Name,
                                    Arguments = tool.Input
                                }
                            });

                            // 도구 결과 메시지
                            tml.Add(new ToolMessage
                            {
                                ID = tool.Id ?? string.Empty,
                                Content = tool.Output ?? string.Empty
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    messages.Add(am);
                    messages.AddRange(tml);
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return new ChatCompletionRequest
        {
            Model = request.Model,
            Messages = messages,
            MaxCompletionTokens = request.MaxTokens,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = isReasoning ? null : request.Temperature,
            TopP = isReasoning ? null : request.TopP,
            Stop = request.StopSequences,
            ReasoningEffort = request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => ReasoningEffort.Low,
                MessageThinkingEffort.Medium => ReasoningEffort.Medium,
                MessageThinkingEffort.High => ReasoningEffort.High,
                _ => null
            },
            Tools = request.Tools.Select(t => new Tool
            {
                Function = new Function
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.Parameters
                }
            }),
        };
    }

    // 추론모델인지 확인합니다.
    private static bool IsReasoningModel (MessageGenerationRequest request)
    {
        // 'o'와 숫자로 시작하는 경우
        return request.Model.StartsWith('o') && char.IsDigit(request.Model[1]);
    }
}
