using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using OpenAI;
using OpenAI.Chat;
using Tiktoken;
using Tiktoken.Encodings;
using IronHiveMessage = IronHive.Abstractions.Messages.Message;
using IronHiveMessageRole = IronHive.Abstractions.Messages.MessageRole;
using TextMessageContent = IronHive.Abstractions.Messages.Content.TextMessageContent;
using ImageMessageContent = IronHive.Abstractions.Messages.Content.ImageMessageContent;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Message generator targeting the OpenAI <b>Chat Completions</b> API (<c>POST /v1/chat/completions</c>).
/// Selected when <see cref="OpenAIConfig.Api"/> is <see cref="OpenAIApiSurface.ChatCompletions"/> — the default
/// for OpenAI-compatible / self-hosted endpoints (Ollama, LM Studio, vLLM, llama.cpp server, GPUStack) that do
/// not implement the Responses API. For first-party OpenAI reasoning use <see cref="OpenAIResponseMessageGenerator"/>.
/// <para>
/// The Chat Completions surface has no reasoning-input block, so assistant <c>thinking</c> content from prior
/// turns is not replayed. <see cref="MessageGenerationRequest.ThinkingEffort"/> is not mapped here (compatible
/// servers generally reject <c>reasoning_effort</c>); use the Responses surface for reasoning.
/// </para>
/// </summary>
public class OpenAIChatMessageGenerator : IMessageGenerator
{
    private readonly OpenAIClient _client;

    // Local token estimate. Accurate for OpenAI models; an approximation for arbitrary compatible models,
    // whose exact tokenizer the Chat Completions surface does not expose. See CountTokensAsync.
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIChatMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIChatMessageGenerator(OpenAIConfig config)
    {
        _client = OpenAIClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(request);
        var options = BuildOptions(request);
        var chatClient = _client.GetChatClient(request.Model);

        var result = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
        var completion = result.Value;

        var content = new List<MessageContent>();

        foreach (var part in completion.Content)
        {
            if (!string.IsNullOrEmpty(part.Text))
                content.Add(new TextMessageContent { Value = part.Text });
        }

        foreach (var tool in completion.ToolCalls)
        {
            content.Add(new ToolMessageContent
            {
                Id = tool.Id,
                Name = tool.FunctionName,
                Input = tool.FunctionArguments?.ToString() ?? string.Empty,
                IsApproved = request.Tools?.TryGet(tool.FunctionName, out var t) != true || t?.RequiresApproval == false
            });
        }

        var reason = MapFinishReason(completion.FinishReason);
        if (content.OfType<ToolMessageContent>().Any())
            reason = MessageDoneReason.ToolCall;

        return new MessageResponse
        {
            ResponseId = completion.Id,
            DoneReason = reason,
            Message = new IronHiveMessage
            {
                Role = IronHiveMessageRole.Assistant,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = completion.Usage?.InputTokenCount ?? 0,
                OutputTokens = completion.Usage?.OutputTokenCount ?? 0
            },
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(request);
        var options = BuildOptions(request);
        var chatClient = _client.GetChatClient(request.Model);

        var reason = MessageDoneReason.EndTurn;
        var usage = new MessageTokenUsage();
        var begun = false;

        // Chat Completions streams text as raw deltas and tool calls keyed by SDK index; there are no explicit
        // block start/stop events. We synthesize the IronHive content-block protocol: assign each text/tool block
        // a sequential content index (Added before its Deltas), then emit Completed for every opened block at the
        // end. MessageService aggregates by that sequential index (stack.Add / stack.ElementAt).
        var textIndex = -1;
        var toolIndexMap = new Dictionary<int, int>();
        var openIndexes = new List<int>();
        var nextIndex = 0;

        await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, options, cancellationToken))
        {
            if (!begun)
            {
                begun = true;
                yield return new StreamingMessageBeginResponse();
            }

            foreach (var part in update.ContentUpdate)
            {
                if (string.IsNullOrEmpty(part.Text))
                    continue;

                if (textIndex < 0)
                {
                    textIndex = nextIndex++;
                    openIndexes.Add(textIndex);
                    yield return new StreamingContentAddedResponse
                    {
                        Index = textIndex,
                        Content = new TextMessageContent { Value = string.Empty }
                    };
                }

                yield return new StreamingContentDeltaResponse
                {
                    Index = textIndex,
                    Delta = new TextDeltaContent { Value = part.Text }
                };
            }

            foreach (var tcu in update.ToolCallUpdates)
            {
                if (!toolIndexMap.TryGetValue(tcu.Index, out var ci))
                {
                    ci = nextIndex++;
                    toolIndexMap[tcu.Index] = ci;
                    openIndexes.Add(ci);
                    reason = MessageDoneReason.ToolCall;
                    yield return new StreamingContentAddedResponse
                    {
                        Index = ci,
                        Content = new ToolMessageContent
                        {
                            Id = tcu.ToolCallId,
                            Name = tcu.FunctionName ?? string.Empty,
                            IsApproved = request.Tools?.TryGet(tcu.FunctionName ?? string.Empty, out var t) != true || t?.RequiresApproval == false
                        }
                    };
                }

                var argsChunk = tcu.FunctionArgumentsUpdate?.ToString();
                if (!string.IsNullOrEmpty(argsChunk))
                {
                    yield return new StreamingContentDeltaResponse
                    {
                        Index = ci,
                        Delta = new ToolDeltaContent { Input = argsChunk }
                    };
                }
            }

            if (update.FinishReason.HasValue && reason != MessageDoneReason.ToolCall)
                reason = MapFinishReason(update.FinishReason.Value);

            if (update.Usage != null)
            {
                usage.InputTokens = update.Usage.InputTokenCount;
                usage.OutputTokens = update.Usage.OutputTokenCount;
            }
        }

        foreach (var idx in openIndexes)
        {
            yield return new StreamingContentCompletedResponse { Index = idx };
        }

        yield return new StreamingMessageDoneResponse
        {
            DoneReason = reason,
            TokenUsage = usage,
        };
    }

    /// <inheritdoc />
    public Task<int> CountTokensAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Chat Completions exposes no server-side token count endpoint (and compatible servers vary), so we
        // estimate locally with tiktoken. Exact for OpenAI models; an approximation elsewhere. Follows the
        // per-message overhead convention from the OpenAI cookbook (~4 tokens/message + priming).
        const int PerMessageOverhead = 4;
        var total = 3; // reply priming

        if (!string.IsNullOrEmpty(request.System))
            total += PerMessageOverhead + _tokenizer.CountTokens(request.System);

        foreach (var msg in request.Messages)
        {
            total += PerMessageOverhead;
            foreach (var item in msg.Content)
            {
                switch (item)
                {
                    case TextMessageContent text when !string.IsNullOrEmpty(text.Value):
                        total += _tokenizer.CountTokens(text.Value);
                        break;
                    case ToolMessageContent tool:
                        if (!string.IsNullOrEmpty(tool.Name)) total += _tokenizer.CountTokens(tool.Name);
                        if (!string.IsNullOrEmpty(tool.Input)) total += _tokenizer.CountTokens(tool.Input);
                        if (!string.IsNullOrEmpty(tool.Output?.Result)) total += _tokenizer.CountTokens(tool.Output!.Result);
                        break;
                }
            }
        }

        if (request.Tools != null)
        {
            foreach (var t in request.Tools)
            {
                if (!string.IsNullOrEmpty(t.UniqueName)) total += _tokenizer.CountTokens(t.UniqueName);
                if (!string.IsNullOrEmpty(t.Description)) total += _tokenizer.CountTokens(t.Description);
                if (t.Parameters != null) total += _tokenizer.CountTokens(JsonSerializer.Serialize(t.Parameters));
            }
        }

        return Task.FromResult(total);
    }

    internal static ChatCompletionOptions BuildOptions(MessageGenerationRequest request)
    {
        var options = new ChatCompletionOptions();

        if (request.MaxTokens.HasValue)
            options.MaxOutputTokenCount = request.MaxTokens.Value;

        if (request.Output?.Type is { } outputType)
        {
            options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                outputType.Name,
                BinaryData.FromObjectAsJson(IronHive.Abstractions.Json.JsonSchemaFactory.Build(outputType)),
                null,
                jsonSchemaIsStrict: false);
        }
        else if (request.Output?.Schema is { } outputSchema)
        {
            options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "output",
                BinaryData.FromString(outputSchema),
                null,
                jsonSchemaIsStrict: false);
        }

        if (request.Tools != null)
        {
            foreach (var t in request.Tools)
            {
                var parameters = t.Parameters ?? new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject()
                };
                options.Tools.Add(ChatTool.CreateFunctionTool(
                    t.UniqueName,
                    t.Description,
                    BinaryData.FromString(JsonSerializer.Serialize(parameters)),
                    functionSchemaIsStrict: false));
            }
        }

        return options;
    }

    internal static List<ChatMessage> BuildMessages(MessageGenerationRequest request)
    {
        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(request.System))
            messages.Add(new SystemChatMessage(request.System));

        foreach (var msg in request.Messages)
        {
            if (msg is { Role: IronHiveMessageRole.User } user)
            {
                var parts = new List<ChatMessageContentPart>();
                foreach (var item in user.Content)
                {
                    if (item is TextMessageContent text)
                    {
                        parts.Add(ChatMessageContentPart.CreateTextPart(text.Value ?? string.Empty));
                    }
                    else if (item is ImageMessageContent image)
                    {
                        parts.Add(ChatMessageContentPart.CreateImagePart(
                            new Uri(EnsureBase64Url(image)),
                            ChatImageDetailLevel.Auto));
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                messages.Add(new UserChatMessage(parts));
            }
            else if (msg is { Role: IronHiveMessageRole.Assistant } assistant)
            {
                var textParts = new List<ChatMessageContentPart>();
                var toolCalls = new List<ChatToolCall>();
                var toolOutputs = new List<(string Id, string Output)>();

                foreach (var group in assistant.GroupContentByToolBoundary())
                {
                    foreach (var content in group)
                    {
                        if (content is ThinkingMessageContent)
                        {
                            // Chat Completions has no reasoning-input block; prior thinking is not replayed.
                        }
                        else if (content is TextMessageContent text)
                        {
                            textParts.Add(ChatMessageContentPart.CreateTextPart(text.Value ?? string.Empty));
                        }
                        else if (content is ToolMessageContent tool)
                        {
                            var id = tool.Id ?? string.Empty;
                            toolCalls.Add(ChatToolCall.CreateFunctionToolCall(
                                id,
                                tool.Name,
                                BinaryData.FromString(tool.Input ?? string.Empty)));
                            toolOutputs.Add((id, tool.Output?.Result ?? string.Empty));
                        }
                        else
                        {
                            throw new NotImplementedException("not supported yet");
                        }
                    }
                }

                if (toolCalls.Count > 0)
                {
                    var assistantMessage = new AssistantChatMessage(toolCalls);
                    foreach (var tp in textParts)
                        assistantMessage.Content.Add(tp);
                    messages.Add(assistantMessage);

                    foreach (var (id, output) in toolOutputs)
                        messages.Add(new ToolChatMessage(id, output));
                }
                else if (textParts.Count > 0)
                {
                    messages.Add(new AssistantChatMessage(textParts));
                }
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return messages;
    }

    internal static MessageDoneReason MapFinishReason(ChatFinishReason reason) => reason switch
    {
        ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
        ChatFinishReason.FunctionCall => MessageDoneReason.ToolCall,
        ChatFinishReason.Stop => MessageDoneReason.EndTurn,
        ChatFinishReason.Length => MessageDoneReason.MaxTokens,
        ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
        _ => MessageDoneReason.Unknown,
    };

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
