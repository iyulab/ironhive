using System.Text.Json;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Providers.Anthropic.Messages;
using TextMessageContent = IronHive.Abstractions.Message.Content.TextMessageContent;
using ThinkingMessageContent = IronHive.Abstractions.Message.Content.ThinkingMessageContent;
using AnthropicMessage = IronHive.Providers.Anthropic.Messages.Message;
using AnthropicTextMessageContent = IronHive.Providers.Anthropic.Messages.TextMessageContent;
using AnthropicThinkingMessageContent = IronHive.Providers.Anthropic.Messages.ThinkingMessageContent;

namespace IronHive.Providers.Anthropic;

internal static class MessageGenerationRequestExtensions
{
    internal static MessagesRequest ToAnthropic(this MessageGenerationRequest request)
    {
        var isThinking = request.Model.StartsWith("claude-3-7-sonnet", StringComparison.OrdinalIgnoreCase)
            || request.Model.StartsWith("claude-sonnet-4", StringComparison.OrdinalIgnoreCase)
            || request.Model.StartsWith("claude-opus-4", StringComparison.OrdinalIgnoreCase);

        var messages = new List<AnthropicMessage>();
        foreach (var message in request.Messages)
        {
            // 사용자 메시지
            if (message is UserMessage user)
            {
                var um = new AnthropicMessage(MessageRole.User);
                foreach (var item in user.Content)
                {
                    if (item is TextMessageContent text)
                    {
                        um.Content.Add(new AnthropicTextMessageContent
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    else if (item is FileMessageContent file)
                    {
                        throw new NotSupportedException("not supported file yet");
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
                var groups = assistant.SplitContentByTool();
                
                foreach (var group in groups)
                {
                    var am = new AnthropicMessage(MessageRole.Assistant);
                    var um = new AnthropicMessage(MessageRole.User);

                    // 추론 모델은 툴 사용 시나리오에서
                    // ToolUse 이전에 반드시 추론 메시지가 존재해야 합니다.
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
                                    ? JsonSerializer.Deserialize<IDictionary<string, object>>(tool.Input)
                                    : new object()
                            });
                            um.Content.Add(new ToolResultMessageContent
                            {
                                ToolUseId = tool.Id,
                                IsError = !tool.Output?.IsSuccess ?? false,
                                Content = tool.Output?.Data ?? string.Empty,
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

        return new MessagesRequest
        {
            Model = request.Model,
            System = request.System,
            Messages = messages,
            // MaxToken이 필수요청사항으로 "8192"값을 기본으로 함
            MaxTokens = request.Parameters?.MaxTokens ?? (isThinking ? 64_000 : 8_192),
            Temperature = request.Parameters?.Temperature,
            TopK = request.Parameters?.TopK,
            TopP = request.Parameters?.TopP,
            StopSequences = request.Parameters?.StopSequences,
            // 추론 모델일 경우 기본 사용
            Thinking = isThinking
            ? new EnabledThinking { BudgetTokens = 32_000 }
            : null,
            Tools = request.Tools.Select(t => new CustomTool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.Parameters ?? new ObjectJsonSchema()
            })
        };
    }
}
