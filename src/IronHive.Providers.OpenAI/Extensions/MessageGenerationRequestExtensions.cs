using System.Text.Json.Nodes;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI.Payloads.Responses;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;
using UserMessage = IronHive.Abstractions.Messages.Roles.UserMessage;
using AssistantMessage = IronHive.Abstractions.Messages.Roles.AssistantMessage;
using OpenAIMessage = IronHive.Providers.OpenAI.Payloads.ChatCompletion.ChatMessage;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;

namespace IronHive.Abstractions.Message;

public static class MessageGenerationRequestExtensions
{
    /// <summary>
    /// 메시지 생성 요청을 OpenAI의 ResponsesRequest로 변환합니다.
    /// </summary>
    internal static ResponsesRequest ToOpenAI(this MessageGenerationRequest request)
    {
        var items = new List<ResponsesItem>();

        // xAI grok-4 모델은 instructions를 지원하지 않음
        // 대신 system 메시지를 input의 첫 번째로 추가해야 함
        var modelLower = request.Model.ToLowerInvariant();
        var isXai = modelLower.StartsWith("grok-");

        if (isXai && !string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            items.Add(new ResponsesMessageItem
            {
                Role = ResponsesMessageRole.System,
                Content = [new ResponsesInputTextContent { Text = request.SystemPrompt }]
            });
        }

        foreach (var msg in request.Messages)
        {
            if (msg is UserMessage user)
            {
                // 사용자 메시지
                var um = new ResponsesMessageItem
                {
                    Role = ResponsesMessageRole.User,
                    Content = [],
                };
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        um.Content.Add(new ResponsesInputTextContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        um.Content.Add(new ResponsesInputFileContent
                        {
                            FileData = document.Data,
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new ResponsesInputImageContent
                        {
                            Detail = "auto",
                            ImageUrl = image.Base64,
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                items.Add(um);
            }
            else if (msg is AssistantMessage assistant)
            {
                // AI 메시지
                foreach (var group in assistant.GroupContentByToolBoundary())
                {
                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent thinking)
                        {
                            items.Add(new ResponsesReasoningItem
                            {
                                EncryptedContent = thinking.Signature,
                                Summary = string.IsNullOrWhiteSpace(thinking.Value) ? [] :
                                [ 
                                    new ResponsesReasoningContent 
                                    { 
                                        Type = "summary_text",
                                        Text = thinking.Value
                                    }
                                ],
                            });
                        }
                        else if (content is TextMessageContent text)
                        {
                            items.Add(new ResponsesMessageItem
                            {
                                Role = ResponsesMessageRole.Assistant,
                                Content = [
                                    new ResponsesOutputTextContent
                                    {
                                        Text = text.Value ?? string.Empty
                                    }
                                ]
                            });
                        }
                        else if (content is ToolMessageContent tool)
                        {
                            // 도구 호출 메시지
                            items.Add(new ResponsesFunctionToolCallItem
                            {
                                CallId = tool.Id,
                                Name = tool.Name,
                                Arguments = tool.Input ?? string.Empty,
                            });

                            // 도구 결과 메시지
                            items.Add(new ResponsesFunctionToolOutputItem
                            {
                                CallId = tool.Id,
                                Output = tool.Output?.Result ?? string.Empty
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        var enabledReasoning = request.ThinkingEffort != MessageThinkingEffort.None;

        // xAI grok-4 모델은 reasoning_effort를 지원하지 않음 (grok-3-mini만 지원)
        // 하지만 include: ["reasoning.encrypted_content"]는 필요
        var isGrok4 = modelLower.StartsWith("grok-4");
        var supportsReasoningEffort = !isGrok4;

        return new ResponsesRequest
        {
            Model = request.Model,
            // xAI는 instructions를 지원하지 않음 (위에서 system 메시지로 추가됨)
            Instructions = isXai ? null : request.SystemPrompt,
            Input = items,
            MaxOutputTokens = request.MaxTokens,
            Tools = request.Tools?.Select(t => new ResponsesFunctionTool
            {
                Name = t.UniqueName,
                Description = t.Description,
                Parameters = t.Parameters ?? new JsonObject()
            }),
            Include = enabledReasoning ? [ "reasoning.encrypted_content" ] : null,
            Reasoning = enabledReasoning && supportsReasoningEffort ? new ResponsesReasoning
            {
                Effort = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.Low => ResponsesReasoningEffort.Low,
                    MessageThinkingEffort.Medium => ResponsesReasoningEffort.Medium,
                    MessageThinkingEffort.High => ResponsesReasoningEffort.High,
                    _ => ResponsesReasoningEffort.Low
                },
                Summary = ResponsesReasoningSummary.Detailed,
            }: null,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = enabledReasoning && supportsReasoningEffort ? null : request.Temperature,
            TopP = enabledReasoning && supportsReasoningEffort ? null : request.TopP,
        };
    }

    /// <summary>
    /// 메시지 생성 요청을 OpenAI의 ChatCompletionRequest로 변환합니다.
    /// </summary>
    public static ChatCompletionRequest ToOpenAILegacy(this MessageGenerationRequest request)
    {
        // reasoning_effort는 o-series와 gpt-5 이상 모델만 지원
        var model = request.Model.ToLowerInvariant();
        var supportsReasoning = model.StartsWith("o1") || model.StartsWith("o3") || model.StartsWith("o4")
                             || model.StartsWith("gpt-5") || model.Contains("-o1") || model.Contains("-o3");
        var enabledReasoning = supportsReasoning && request.ThinkingEffort is not null and not MessageThinkingEffort.None;
        var messages = new List<OpenAIMessage>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            if (enabledReasoning)
                messages.Add(new DeveloperChatMessage { Content = request.SystemPrompt });
            else
                messages.Add(new SystemChatMessage { Content = request.SystemPrompt });
        }

        foreach (var message in request.Messages)
        {
            if (message is UserMessage user)
            {
                // 사용자 메시지
                var um = new UserChatMessage();
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        um.Content.Add(new TextChatMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        um.Content.Add(new TextChatMessageContent
                        {
                            Text = $"Document Content:\n{document.Data}"
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new ImageChatMessageContent
                        {
                            ImageUrl = new ImageChatMessageContent.ImageSource
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
                foreach (var group in assistant.GroupContentByToolBoundary())
                {
                    var am = new AssistantChatMessage();
                    var tml = new List<ToolChatMessage>();
                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent thinking)
                        {
                            // 추론 컨텐츠 건너뛰기(Not Supported OpenAI ChatCompletion)
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
                            am.ToolCalls.Add(new ChatFunctionToolCall
                            {
                                Id = tool.Id,
                                Function = new ChatFunctionToolCall.FunctionSchema
                                {
                                    Name = tool.Name,
                                    Arguments = tool.Input
                                }
                            });

                            // 도구 결과 메시지
                            tml.Add(new ToolChatMessage
                            {
                                ID = tool.Id ?? string.Empty,
                                Content = tool.Output?.Result ?? string.Empty
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
            Stop = request.StopSequences,
            Tools = request.Tools?.Select(t => new ChatFunctionTool
            {
                Function = new ChatFunctionTool.FunctionSchema
                {
                    Name = t.UniqueName,
                    Description = t.Description,
                    Parameters = t.Parameters
                }
            }),
            ReasoningEffort = enabledReasoning ? request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => ChatReasoningEffort.Low,
                MessageThinkingEffort.Medium => ChatReasoningEffort.Medium,
                MessageThinkingEffort.High => ChatReasoningEffort.High,
                _ => null
            } : null,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = enabledReasoning ? null : request.Temperature,
            TopP = enabledReasoning ? null : request.TopP,
        };
    }
}