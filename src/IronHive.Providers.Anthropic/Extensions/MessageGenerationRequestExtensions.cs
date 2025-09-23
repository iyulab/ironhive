﻿using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using IronHive.Providers.Anthropic.Messages;
using AnthropicMessage = IronHive.Providers.Anthropic.Messages.Message;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using AnthropicTextMessageContent = IronHive.Providers.Anthropic.Messages.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;
using DocumentMessageContent = IronHive.Abstractions.Messages.Content.DocumentMessageContent;
using AnthropicImageMessageContent = IronHive.Providers.Anthropic.Messages.ImageMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Messages.Content.ThinkingMessageContent;
using AnthropicThinkingMessageContent = IronHive.Providers.Anthropic.Messages.ThinkingMessageContent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using Json.Schema;

namespace IronHive.Providers.Anthropic;

internal static class MessageGenerationRequestExtensions
{
    internal static MessagesRequest ToAnthropic(this MessageGenerationRequest request)
    {
        var isThinking = IsThinkingModel(request);

        var messages = new List<AnthropicMessage>();
        foreach (var message in request.Messages)
        {
            // 사용자 메시지
            if (message is UserMessage user)
            {
                var um = new AnthropicMessage(MessageRole.User);
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        um.Content.Add(new AnthropicTextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        um.Content.Add(new AnthropicTextMessageContent
                        {
                            Text = $"Document Content:\n{document.Data}"
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Content.Add(new AnthropicImageMessageContent
                        {
                            Source = new Base64ImageSource
                            {
                                MediaType = image.Format switch
                                {
                                    ImageFormat.Png => "image/png",
                                    ImageFormat.Jpeg => "image/jpeg",
                                    ImageFormat.Gif => "image/gif",
                                    ImageFormat.Webp => "image/webp",
                                    _ => throw new NotSupportedException($"not supported image format {image.Format}")
                                },
                                Data = image.Base64
                            }
                        });
                    }
                    else
                    {
                        throw new NotSupportedException($"not supported type {item.GetType()}");
                    }
                }

                messages.Add(um);
            }
            // AI 메시지
            else if (message is AssistantMessage assistant)
            {
                var groups = assistant.GroupContentByToolBoundary();
                foreach (var group in groups)
                {
                    var am = new AnthropicMessage(MessageRole.Assistant);
                    var um = new AnthropicMessage(MessageRole.User);

                    // 추론 모델은 툴 사용 시나리오에서
                    // ToolUseMessageContent 이전에 반드시 추론 메시지가 존재해야 합니다.
                    var isLastMessage = message == request.Messages.Last();
                    var isFirstGroup = group == groups.First();
                    if (isLastMessage && isFirstGroup && isThinking)
                    {
                        foreach (var thinking in group.OfType<ThinkingMessageContent>())
                        {
                            am.Content.Add(thinking.Format == ThinkingFormat.Secure
                                ? new RedactedThinkingMessageContent
                                {
                                    Data = thinking.Value ?? string.Empty,
                                }
                                : new AnthropicThinkingMessageContent
                                {
                                    Signature = thinking.Id ?? string.Empty,
                                    Thinking = thinking.Value ?? string.Empty
                                });
                        }
                    }

                    foreach (var content in group)
                    {
                        // 추론 메시지
                        if (content is ThinkingMessageContent)
                        {
                            continue;
                        }
                        // 텍스트 메시지
                        else if (content is TextMessageContent text)
                        {
                            am.Content.Add(new AnthropicTextMessageContent
                            {
                                Text = text.Value ?? string.Empty
                            });
                        }
                        // 도구 메시지
                        else if (content is ToolMessageContent tool)
                        {
                            am.Content.Add(new ToolUseMessageContent
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Input = !string.IsNullOrWhiteSpace(tool.Input)
                                    ? new ToolInput(tool.Input)
                                    : new object()
                            });
                            um.Content.Add(new ToolResultMessageContent
                            {
                                ToolUseId = tool.Id,
                                IsError = tool.Output != null && !tool.Output.IsSuccess,
                                Content = tool.Output?.Result ?? string.Empty,
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    messages.Add(am);
                    if (um.Content.Count > 0)
                        messages.Add(um);
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        // 필수요청사항인 토큰을 계산합니다.
        var (maxTokens, budgetTokens) = CaculateTokens(request);
        return new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = messages,
            MaxTokens = maxTokens,
            // 추론 모델의 경우 토큰샘플링 방식을 임의로 설정할 수 없습니다.
            Temperature = isThinking ? null : request.Temperature,
            TopP = isThinking ? null : request.TopP,
            TopK = isThinking ? null : request.TopK,
            StopSequences = request.StopSequences,
            Thinking = budgetTokens,
            Tools = request.Tools?.Select(t => new CustomAnthropicTool
            {
                Name = t.UniqueName,
                Description = t.Description,
                InputSchema = t.Parameters ?? JsonSchema.FromText("{}")
            })
        };
    }

    // 추론 모델인지 확인합니다.
    internal static bool IsThinkingModel(MessageGenerationRequest request)
    {
        return request.Model.StartsWith("claude-3-7-sonnet", StringComparison.OrdinalIgnoreCase)
            || request.Model.StartsWith("claude-sonnet-4", StringComparison.OrdinalIgnoreCase)
            || request.Model.StartsWith("claude-opus-4", StringComparison.OrdinalIgnoreCase);
    }

    // 최대 토큰 수를 계산하고 추론 예산을 설정합니다.
    internal static (int, EnabledThinkingEffort?) CaculateTokens(MessageGenerationRequest request)
    {
        // 추론 모델의 경우 예산 토큰을 설정합니다.
        if (IsThinkingModel(request))
        {
            // MaxToken이 필수요청사항으로 opus는 "32000", sonnet은 "64000"을 기본으로 함
            var maxTokens = request.MaxTokens ??
                (request.Model.StartsWith("claude-opus-4") ? 32_000 : 64_000);

            if (request.ThinkingEffort == null)
                return (maxTokens, null);

            // 절대값 기반 + 비례값 기반 혼합 전략
            int budgetTokens = (int)(request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => Math.Min(maxTokens * 0.25, 12_000),    // 25%, 12K
                MessageThinkingEffort.Medium => Math.Min(maxTokens * 0.50, 32_000), // 50%, 32K  
                MessageThinkingEffort.High => Math.Min(maxTokens * 0.75, 48_000),   // 75%, 48K
                _ => 1024 // 최소값
            });
            // "1024"토큰보다 커야함
            budgetTokens = budgetTokens >= 1024 ? budgetTokens : 1024;
            return (maxTokens, new EnabledThinkingEffort { BudgetTokens = budgetTokens });
        }
        else
        {
            // MaxToken이 필수요청사항으로 기본모델은 "8192"값을 기본으로 함
            var maxTokens = request.MaxTokens ?? 8192;
            return (maxTokens, null);
        }
    }
}
