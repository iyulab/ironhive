using System.Text.Json.Nodes;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Providers.OpenAI.Payloads.Responses;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;
using UserMessage = IronHive.Abstractions.Messages.Roles.UserMessage;
using AssistantMessage = IronHive.Abstractions.Messages.Roles.AssistantMessage;
using OpenAIMessage = IronHive.Providers.OpenAI.Payloads.ChatCompletion.ChatMessage;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;

namespace IronHive.Providers.OpenAI;

public static class MessageRequestExtensions
{
    /// <summary>
    /// 메시지 생성 요청을 OpenAI의 ResponsesRequest로 변환합니다.
    /// </summary>
    public static ResponsesRequest ToOpenAI(this MessageGenerationRequest request)
    {
        var items = new List<ResponsesItem>();

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
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new ResponsesInputImageContent
                        {
                            Detail = "auto",
                            ImageUrl = EnsureBase64Url(image),
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

        var enabledReasoning = request.ThinkingEffort != null && request.ThinkingEffort != MessageThinkingEffort.None;
        return new ResponsesRequest
        {
            Model = request.Model,
            Instructions = request.System,
            Input = items,
            MaxOutputTokens = request.MaxTokens,
            Text = request.Output != null ? new ResponsesText
            {
                Format = new JsonSchemaResponseFormat
                {
                    Name = request.Output.Name,
                    Schema = JsonSchemaFactory.Build(request.Output),
                    Strict = false,
                }
            } : null,
            Tools = request.Tools?.Select(t => new ResponsesFunctionTool
            {
                Name = t.UniqueName,
                Description = t.Description,
                Parameters = t.Parameters ?? new JsonObject 
                { 
                    ["type"] = "object", 
                    ["properties"] = new JsonObject() 
                }
            }),
            Include = enabledReasoning ? 
            [
                "reasoning.encrypted_content" 
            ] : null,
            Reasoning = request.ThinkingEffort != null ? new ResponsesReasoning
            {
                Effort = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.None => ResponsesReasoningEffort.None,
                    MessageThinkingEffort.Minimal => ResponsesReasoningEffort.Minimal,
                    MessageThinkingEffort.Low => ResponsesReasoningEffort.Low,
                    MessageThinkingEffort.Medium => ResponsesReasoningEffort.Medium,
                    MessageThinkingEffort.High => ResponsesReasoningEffort.High,
                    MessageThinkingEffort.XHigh => ResponsesReasoningEffort.Xhigh,
                    _ => ResponsesReasoningEffort.Low
                },
                Summary = enabledReasoning ? ResponsesReasoningSummary.Detailed : null
            }: null,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = enabledReasoning ? null : request.Temperature,
            TopP = enabledReasoning ? null : request.TopP,
        };
    }

    /// <summary>
    /// 메시지 생성 요청을 OpenAI의 ChatCompletionRequest로 변환합니다.
    /// </summary>
    public static ChatCompletionRequest ToOpenAILegacy(this MessageGenerationRequest request, bool openai = true)
    {
        var messages = new List<OpenAIMessage>();
        if (!string.IsNullOrWhiteSpace(request.System))
        {
            messages.Add(new SystemChatMessage { Content = request.System });
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
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new ImageChatMessageContent
                        {
                            ImageUrl = new ImageChatMessageContent.ImageSource
                            {
                                Url = EnsureBase64Url(image)
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
                                    Arguments = tool.Input ?? "{}"
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

        var enabledReasoning = request.ThinkingEffort != null && request.ThinkingEffort != MessageThinkingEffort.None;
        return new ChatCompletionRequest
        {
            Model = request.Model!,
            Messages = messages,
            MaxCompletionTokens = request.MaxTokens,
            ResponseFormat = request.Output != null ? new JsonSchemaChatResponseFormat
            {
                JsonSchema = new JsonSchemaChatResponseFormat.JsonFormat
                {
                    Name = request.Output.Name,
                    Schema = JsonSchemaFactory.Build(request.Output),
                    Strict = false,
                }
            }
            : null,
            Tools = request.Tools?.Select(t => new ChatFunctionTool
            {
                Function = new ChatFunctionTool.FunctionSchema
                {
                    Name = t.UniqueName,
                    Description = t.Description,
                    Parameters = t.Parameters ?? new JsonObject 
                    { 
                        ["type"] = "object", 
                        ["properties"] = new JsonObject() 
                    }
                }
            }),
            ReasoningEffort = request.ThinkingEffort != null ? request.ThinkingEffort switch
            {
                MessageThinkingEffort.None => ChatReasoningEffort.None,
                MessageThinkingEffort.Minimal => ChatReasoningEffort.Minimal,
                MessageThinkingEffort.Low => ChatReasoningEffort.Low,
                MessageThinkingEffort.Medium => ChatReasoningEffort.Medium,
                MessageThinkingEffort.High => ChatReasoningEffort.High,
                MessageThinkingEffort.XHigh => ChatReasoningEffort.Xhigh,
                _ => null
            } : null,
            Temperature = !enabledReasoning ? request.Temperature : null,
            TopP = !enabledReasoning ? request.TopP : null,
            ExtraBody = openai == false ? new JsonObject
            {
                // vLLM 추가 파라미터
                // https://docs.vllm.ai/en/latest/features/reasoning_outputs/

                // Supports Budget "Qwen", "DeepSeek", "Nemotron"
                ["thinking_token_budget"] = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.None => 0,           // 추론 비활성화
                    MessageThinkingEffort.Minimal => 256,      // 아주 간단한 정정이나 확인
                    MessageThinkingEffort.Low => 512,          // 일반적인 대화 내 가벼운 논리 흐름
                    MessageThinkingEffort.Medium => 1024,      // 프로그래밍 디버깅, 간단한 수학
                    MessageThinkingEffort.High => 2048,        // 복잡한 알고리즘, 논리 추론
                    MessageThinkingEffort.XHigh => 4096,       // 심층 분석, 긴 단계의 다중 추론
                    _ => 0
                },
                // Server Default Override
                ["chat_template_kwargs"] = new JsonObject
                {
                    ["thinking"] = enabledReasoning,           // DeepSeek, IBM Granite
                    ["enable_thinking"] = enabledReasoning,    // Qwen
                }
            } : null
        };
    }

    /// <summary>Ensures that the image content is in a valid Base64 URL format.</summary>
    private static string EnsureBase64Url(ImageMessageContent image)
    {
        if (image.Base64.StartsWith("data:", StringComparison.Ordinal))
            return image.Base64;

        var mime = image.Format switch
        {
            ImageFormat.Png => "image/png",
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Webp => "image/webp",
            _ => throw new NotSupportedException($"Unsupported image format: {image.Format}")
        };
        return $"data:{mime};base64,{image.Base64}";
    }
}
