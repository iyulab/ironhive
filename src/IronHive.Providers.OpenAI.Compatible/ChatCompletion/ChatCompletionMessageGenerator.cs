using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHiveMessage = IronHive.Abstractions.Messages.Message;
using IronHiveMessageRole = IronHive.Abstractions.Messages.MessageRole;
using ChatMessage = IronHive.Providers.OpenAI.Compatible.ChatCompletion.ChatMessage;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Message generator targeting the OpenAI <b>Chat Completions</b> API (<c>POST /v1/chat/completions</c>) — the
/// de-facto standard OpenAI-compatible / self-hosted servers (Ollama, LM Studio, vLLM, llama.cpp server, GPUStack)
/// implement. Used exclusively by this package (<see cref="OpenAICompatibleMessageGenerator"/>,
/// <see cref="GpuStack.GpuStackMessageGenerator"/>); first-party OpenAI always uses the Responses API
/// (<see cref="IronHive.Providers.OpenAI.OpenAIMessageGenerator"/>) instead.
/// <para>
/// Talks raw HTTP/JSON (<see cref="ChatCompletionHttpClient"/>) rather than the OpenAI SDK. Compatible servers
/// commonly emit a <c>reasoning_content</c> (or <c>reasoning</c>) field on reasoning-capable models that the
/// SDK's typed models have no slot for and no raw-JSON escape hatch to recover during streaming — see
/// <see cref="ChatCompletionHttpClient"/> for details. Owning the JSON directly also lets vendor sampling
/// extensions (e.g. vLLM's <c>thinking_token_budget</c>) ride along in the request body.
/// </para>
/// </summary>
public class ChatCompletionMessageGenerator : IMessageGenerator
{
    private readonly ChatCompletionHttpClient _client;

    public ChatCompletionMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public ChatCompletionMessageGenerator(OpenAIConfig config)
    {
        _client = new ChatCompletionHttpClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(request);
        var res = await _client.PostAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var content = new List<MessageContent>();

        var reasoning = ExtractReasoning(res.ExtraBody, "message");
        if (!string.IsNullOrWhiteSpace(reasoning))
            content.Add(new ThinkingMessageContent { Value = reasoning });

        var text = choice?.Message?.Content;
        if (!string.IsNullOrWhiteSpace(text))
            content.Add(new TextMessageContent { Value = text });

        foreach (var tool in choice?.Message?.ToolCalls ?? [])
        {
            var name = tool.Function?.Name ?? string.Empty;
            content.Add(new ToolMessageContent
            {
                Id = tool.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                Name = name,
                Input = tool.Function?.Arguments ?? string.Empty,
                IsApproved = request.Tools?.TryGet(name, out var t) != true || t?.RequiresApproval == false
            });
        }

        var reason = MapFinishReason(choice?.FinishReason);
        if (content.OfType<ToolMessageContent>().Any())
            reason = MessageDoneReason.ToolCall;

        return new MessageResponse
        {
            ResponseId = res.Id,
            DoneReason = reason,
            Message = new IronHiveMessage
            {
                Role = IronHiveMessageRole.Assistant,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage?.PromptTokens ?? 0,
                OutputTokens = res.Usage?.CompletionTokens ?? 0
            },
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = BuildRequest(request);

        var reason = MessageDoneReason.EndTurn;
        var usage = new MessageTokenUsage();
        var begun = false;

        // Chat Completions streams text as raw deltas and tool calls keyed by index; there are no explicit
        // block start/stop events. We synthesize the IronHive content-block protocol: assign each block a
        // sequential content index (Added before its Deltas). Thinking and text share a single "primary" slot
        // — closed as soon as the other starts, since a reasoning model finishes its reasoning phase before
        // the answer phase begins and the two never interleave mid-stream. Tool calls get independent,
        // never-mid-stream-closed indices (via toolIndexMap) so parallel tool calls streamed out of order stay
        // correctly separated. Anything still open when the stream ends is closed in the final flush.
        var primaryIndex = -1;
        var primaryKind = PrimaryContentKind.None;
        var toolIndexMap = new Dictionary<int, int>();
        var openIndexes = new List<int>();
        var completedIndexes = new HashSet<int>();
        var nextIndex = 0;

        await foreach (var chunk in _client.PostStreamingAsync(req, cancellationToken))
        {
            if (!begun)
            {
                begun = true;
                yield return new StreamingMessageBeginResponse();
            }

            // reasoning_content (or reasoning) delta, surfaced via ExtraBody since no typed model exposes it.
            var reasoningDelta = ExtractReasoning(chunk.ExtraBody, "delta");
            if (!string.IsNullOrEmpty(reasoningDelta))
            {
                if (primaryKind != PrimaryContentKind.Thinking)
                {
                    if (primaryKind != PrimaryContentKind.None)
                    {
                        completedIndexes.Add(primaryIndex);
                        yield return new StreamingContentCompletedResponse { Index = primaryIndex };
                    }

                    primaryIndex = nextIndex++;
                    primaryKind = PrimaryContentKind.Thinking;
                    openIndexes.Add(primaryIndex);
                    yield return new StreamingContentAddedResponse
                    {
                        Index = primaryIndex,
                        Content = new ThinkingMessageContent { Value = string.Empty }
                    };
                }

                yield return new StreamingContentDeltaResponse
                {
                    Index = primaryIndex,
                    Delta = new ThinkingDeltaContent { Data = reasoningDelta }
                };
            }

            var choice = chunk.Choices?.FirstOrDefault();
            var delta = choice?.Delta;

            if (!string.IsNullOrEmpty(delta?.Content))
            {
                if (primaryKind != PrimaryContentKind.Text)
                {
                    if (primaryKind != PrimaryContentKind.None)
                    {
                        completedIndexes.Add(primaryIndex);
                        yield return new StreamingContentCompletedResponse { Index = primaryIndex };
                    }

                    primaryIndex = nextIndex++;
                    primaryKind = PrimaryContentKind.Text;
                    openIndexes.Add(primaryIndex);
                    yield return new StreamingContentAddedResponse
                    {
                        Index = primaryIndex,
                        Content = new TextMessageContent { Value = string.Empty }
                    };
                }

                yield return new StreamingContentDeltaResponse
                {
                    Index = primaryIndex,
                    Delta = new TextDeltaContent { Value = delta.Content }
                };
            }

            if (delta?.ToolCalls is { } toolCalls && toolCalls.Count > 0 && primaryKind != PrimaryContentKind.None)
            {
                completedIndexes.Add(primaryIndex);
                yield return new StreamingContentCompletedResponse { Index = primaryIndex };
                primaryKind = PrimaryContentKind.None;
            }

            foreach (var tcu in delta?.ToolCalls ?? [])
            {
                var toolCallIndex = tcu.Index ?? 0;
                if (!toolIndexMap.TryGetValue(toolCallIndex, out var ci))
                {
                    ci = nextIndex++;
                    toolIndexMap[toolCallIndex] = ci;
                    openIndexes.Add(ci);
                    reason = MessageDoneReason.ToolCall;

                    var name = tcu.Function?.Name ?? string.Empty;
                    yield return new StreamingContentAddedResponse
                    {
                        Index = ci,
                        Content = new ToolMessageContent
                        {
                            Id = tcu.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = name,
                            IsApproved = request.Tools?.TryGet(name, out var t) != true || t?.RequiresApproval == false
                        }
                    };
                }

                var argsChunk = tcu.Function?.Arguments;
                if (!string.IsNullOrEmpty(argsChunk))
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = ci,
                        Delta = new ToolDeltaContent { Input = argsChunk }
                    };
                }
            }

            if (choice?.FinishReason != null && reason != MessageDoneReason.ToolCall)
                reason = MapFinishReason(choice.FinishReason);

            if (chunk.Usage != null)
            {
                usage.InputTokens = chunk.Usage.PromptTokens;
                usage.OutputTokens = chunk.Usage.CompletionTokens;
            }
        }

        foreach (var idx in openIndexes)
        {
            if (completedIndexes.Add(idx))
                yield return new StreamingContentCompletedResponse { Index = idx };
        }

        yield return new StreamingMessageDoneResponse
        {
            DoneReason = reason,
            TokenUsage = usage,
        };
    }

    private enum PrimaryContentKind { None, Thinking, Text }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Always thrown. Chat Completions exposes no token-count endpoint, and compatible servers serve arbitrary
    /// model families (Llama, Qwen, DeepSeek, ...) with unrelated tokenizers — there is no single correct
    /// answer to give here. Override this method with a server-specific implementation if you need one (e.g.
    /// llama.cpp/vLLM's <c>/tokenize</c> endpoint).
    /// </exception>
    public virtual Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"{nameof(ChatCompletionMessageGenerator)} does not support token counting: Chat Completions has " +
            "no token-count endpoint, and compatible servers serve arbitrary model families with unrelated " +
            "tokenizers, so no single implementation here would be accurate. Override CountTokensAsync with " +
            "a server-specific implementation (e.g. llama.cpp/vLLM's /tokenize endpoint) if you need one.");
    }

    internal static ChatCompletionRequest BuildRequest(MessageGenerationRequest request)
    {
        var enabledReasoning = request.ThinkingEffort is not null and not MessageThinkingEffort.None;

        return new ChatCompletionRequest
        {
            Model = request.Model,
            Messages = BuildMessages(request),
            MaxCompletionTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            TopK = request.TopK,
            Stop = request.StopSequences,
            ResponseFormat = BuildResponseFormat(request),
            Tools = request.Tools?.Select(t => new ChatTool
            {
                Function = new ChatTool.FunctionSchema
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
            ReasoningEffort = request.ThinkingEffort switch
            {
                null => null,
                MessageThinkingEffort.None => ChatReasoningEffort.None,
                MessageThinkingEffort.Minimal => ChatReasoningEffort.Minimal,
                MessageThinkingEffort.Low => ChatReasoningEffort.Low,
                MessageThinkingEffort.Medium => ChatReasoningEffort.Medium,
                MessageThinkingEffort.High => ChatReasoningEffort.High,
                MessageThinkingEffort.XHigh => ChatReasoningEffort.Xhigh,
                _ => null
            },
            // Vendor extensions for hybrid-reasoning open-weight models served over Chat Completions.
            // https://docs.vllm.ai/en/latest/features/reasoning_outputs/
            ExtraBody = new JsonObject
            {
                ["thinking_token_budget"] = request.ThinkingEffort switch
                {
                    MessageThinkingEffort.None => 0,
                    MessageThinkingEffort.Minimal => 256,
                    MessageThinkingEffort.Low => 512,
                    MessageThinkingEffort.Medium => 1024,
                    MessageThinkingEffort.High => 2048,
                    MessageThinkingEffort.XHigh => 4096,
                    _ => 0
                },
                ["chat_template_kwargs"] = new JsonObject
                {
                    ["thinking"] = enabledReasoning,        // DeepSeek, IBM Granite
                    ["enable_thinking"] = enabledReasoning,  // Qwen
                }
            }
        };
    }

    private static ChatResponseFormat? BuildResponseFormat(MessageGenerationRequest request)
    {
        if (request.OutputFormat is not { } outputFormat)
            return null;

        return new ChatResponseFormat
        {
            JsonSchema = new ChatResponseFormat.JsonSchemaFormat
            {
                Name = "output",
                Schema = outputFormat.Schema,
                Strict = false,
            }
        };
    }

    internal static List<ChatMessage> BuildMessages(MessageGenerationRequest request)
    {
        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(request.System))
            messages.Add(new SystemChatMessage { Content = request.System });

        foreach (var msg in request.Messages)
        {
            if (msg is { Role: IronHiveMessageRole.User } user)
            {
                var parts = new List<ChatMessageContent>();
                foreach (var item in user.Content)
                {
                    if (item is TextMessageContent text)
                    {
                        parts.Add(new TextChatMessageContent { Text = text.Value ?? string.Empty });
                    }
                    else if (item is ImageMessageContent image)
                    {
                        parts.Add(new ImageChatMessageContent
                        {
                            ImageUrl = new ImageChatMessageContent.ImageSource
                            {
                                Url = EnsureBase64Url(image),
                                Detail = "auto"
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                messages.Add(new UserChatMessage { Content = parts });
            }
            else if (msg is { Role: IronHiveMessageRole.Assistant } assistant)
            {
                foreach (var group in assistant.GroupContentByToolBoundary())
                {
                    string? text = null;
                    List<ChatToolCall>? toolCalls = null;
                    var toolOutputs = new List<(string Id, string Output)>();

                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent)
                        {
                            // Chat Completions has no reasoning-input block; prior thinking is not replayed.
                        }
                        else if (content is TextMessageContent textContent)
                        {
                            text = (text ?? string.Empty) + textContent.Value;
                        }
                        else if (content is ToolMessageContent tool)
                        {
                            var id = tool.Id ?? string.Empty;
                            toolCalls ??= [];
                            toolCalls.Add(new ChatToolCall
                            {
                                Id = id,
                                Function = new ChatToolCall.FunctionCall
                                {
                                    Name = tool.Name,
                                    Arguments = tool.Input ?? "{}"
                                }
                            });
                            toolOutputs.Add((id, tool.Output?.Result ?? string.Empty));
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }

                    if (toolCalls != null)
                    {
                        messages.Add(new AssistantChatMessage { Content = text, ToolCalls = toolCalls });
                        foreach (var (id, output) in toolOutputs)
                            messages.Add(new ToolChatMessage { ToolCallId = id, Content = output });
                    }
                    else if (text != null)
                    {
                        messages.Add(new AssistantChatMessage { Content = text });
                    }
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return messages;
    }

    internal static MessageDoneReason MapFinishReason(ChatFinishReason? reason) => reason switch
    {
        ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
        ChatFinishReason.Stop => MessageDoneReason.EndTurn,
        ChatFinishReason.Length => MessageDoneReason.MaxTokens,
        ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
        _ => MessageDoneReason.Unknown,
    };

    /// <summary>Reads the vendor <c>reasoning_content</c> (or <c>reasoning</c>) field OpenAI's own schema has
    /// no slot for, from the given choice container ("message" for non-streaming, "delta" while streaming).</summary>
    private static string? ExtractReasoning(JsonObject? extraBody, string container)
    {
        var node = extraBody?["choices"]?[0]?[container];
        return node?["reasoning_content"]?.GetValue<string>() ?? node?["reasoning"]?.GetValue<string>();
    }

    private static string EnsureBase64Url(ImageMessageContent image)
    {
        if (image.Base64.StartsWith("data:", StringComparison.Ordinal))
            return image.Base64;

        var format = image.Format switch
        {
            ImageFormat.Png => "image/png",
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Webp => "image/webp",
            _ => throw new NotSupportedException($"Unsupported image format: {image.Format}")
        };
        return $"data:{format};base64,{image.Base64}";
    }
}
