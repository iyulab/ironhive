using System.ClientModel;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;
//using OpenAI;
//using OpenAI.Chat;
//using OpenAI.Responses;
using ChatFinishReason = IronHive.Providers.OpenAI.Payloads.ChatCompletion.ChatFinishReason;

namespace IronHive.Providers.OpenAI;

/// <inheritdoc />
public class OpenAIChatMessageGenerator : IMessageGenerator
{
    private readonly OpenAIChatCompletionClient _client;

    public OpenAIChatMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey})
    { }

    public OpenAIChatMessageGenerator(OpenAIConfig config)
    {
        _client = new OpenAIChatCompletionClient(config);
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
        var req = OnBeforeSend(request, request.ToOpenAILegacy());
        var res = await _client.PostChatCompletionAsync(req, cancellationToken);
        var choice = res.Choices?.FirstOrDefault();
        var content = new List<MessageContent>();

        // 텍스트 생성
        var text = choice?.Message?.Content;

        if (!string.IsNullOrWhiteSpace(text))
        {
            content.Add(new TextMessageContent
            {
                Value = text
            });
        }

        // 툴 호출
        var tools = choice?.Message?.ToolCalls;
        if (tools != null && tools.Count > 0)
        {
            foreach (var tool in tools)
            {
                var tc = new ToolMessageContent
                {
                    IsApproved = true,
                    Id = tool.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                    Name = string.Empty,
                };
                if (tool is ChatFunctionToolCall func)
                {
                    tc.IsApproved = request.Tools?.TryGet(func.Function?.Name!, out var t) != true || t?.RequiresApproval == false;
                    tc.Name = func.Function?.Name ?? string.Empty;
                    tc.Input = func.Function?.Arguments ?? string.Empty;
                }
                else if (tool is ChatCustomToolCall custom)
                {
                    tc.IsApproved = request.Tools?.TryGet(custom.Custom?.Name!, out var t) != true || t?.RequiresApproval == false;
                    tc.Name = custom.Custom?.Name ?? string.Empty;
                    tc.Input = custom.Custom?.Input ?? string.Empty;
                }
                else
                {
                    throw new NotImplementedException("Unsupported tool call type.");
                }
                content.Add(tc);
            }
        }

        return OnAfterReceive(res, new MessageResponse
        {
            Id = res.Id ?? Guid.NewGuid().ToString(),
            DoneReason = choice?.FinishReason switch
            {
                ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
                ChatFinishReason.Stop => MessageDoneReason.EndTurn,
                ChatFinishReason.Length => MessageDoneReason.MaxTokens,
                ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                _ => null
            },
            Message = new AssistantMessage
            {
                Id = res.Id ?? Guid.NewGuid().ToString(),
                Name = null,
                Model = res.Model,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage?.PromptTokens ?? 0,
                OutputTokens = res.Usage?.CompletionTokens ?? 0
            },
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(res.Created).UtcDateTime
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = OnBeforeSend(request, request.ToOpenAILegacy());

        // 인덱스 추적 관리용
        (int, MessageContent)? current = null;
        int? currentToolIndex = null;

        string id = string.Empty;
        string model = string.Empty;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;
        StreamingChatCompletionResponse? lastChunk = null;
        await foreach (var res in _client.PostStreamingChatCompletionAsync(req, cancellationToken))
        {
            lastChunk = res;

            // 메시지 시작
            if (current == null)
            {
                id = res.Id ?? Guid.NewGuid().ToString();
                model = res.Model ?? req.Model;
                await foreach (var rr in OnStreamingReceive(res, new StreamingMessageBeginResponse
                {
                    Id = id
                }))
                {
                    yield return rr;
                }
            }

            // 토큰 사용량(FinishReason 다음 호출)
            if (res.Usage != null)
            {
                usage = new MessageTokenUsage
                {
                    InputTokens = res.Usage.PromptTokens,
                    OutputTokens = res.Usage.CompletionTokens
                };
            }

            // 메시지 확인 및 건너뛰기
            var choice = res.Choices?.FirstOrDefault();
            if (choice == null)
                continue;

            // 종료 메시지
            if (choice.FinishReason != null)
            {
                reason = choice.FinishReason switch
                {
                    ChatFinishReason.ToolCalls => MessageDoneReason.ToolCall,
                    ChatFinishReason.Stop => MessageDoneReason.EndTurn,
                    ChatFinishReason.Length => MessageDoneReason.MaxTokens,
                    ChatFinishReason.ContentFilter => MessageDoneReason.ContentFilter,
                    _ => null
                };
            }

            // 메시지 확인 및 건너뛰기
            var delta = choice.Delta;
            if (delta == null)
                continue;

            // 툴 사용
            var tools = delta.ToolCalls;
            if (tools != null)
            {
                for (var i = 0; i < tools.Count; i++)
                {
                    var func = tools.ElementAt(i);
                    var funcIndex = func.Index ?? i;

                    // 이어서 생성되는 툴 메시지의 경우
                    if (current.HasValue)
                    {
                        var (index, content) = current.Value;
                        if (content is ToolMessageContent toolContent)
                        {
                            // 현재 컨텐츠가 ToolMessageContent이고, 인덱스가 동일한 경우 업데이트
                            if (funcIndex == currentToolIndex)
                            {
                                await foreach (var rr in OnStreamingReceive(res, new StreamingContentDeltaResponse
                                {
                                    Index = index,
                                    Delta = new ToolDeltaContent
                                    {
                                        Input = func.Function?.Arguments ?? string.Empty
                                    }
                                }))
                                {
                                    yield return rr;
                                }
                            }
                            // 현재 컨텐츠가 ToolMessageContent이지만, 인덱스가 다른 경우
                            // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                            else
                            {
                                await foreach (var rr in OnStreamingReceive(res, new StreamingContentCompletedResponse
                                {
                                    Index = index,
                                }))
                                {
                                    yield return rr;
                                }
                                current = (index + 1, new ToolMessageContent
                                {
                                    IsApproved = request.Tools?.TryGet(func.Function?.Name!, out var t) != true || t?.RequiresApproval == false,
                                    Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                                    Name = func.Function?.Name ?? string.Empty,
                                    Input = func.Function?.Arguments,
                                });
                                currentToolIndex = funcIndex;
                                await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                                {
                                    Index = current.Value.Item1,
                                    Content = current.Value.Item2
                                }))
                                {
                                    yield return rr;
                                }
                            }
                        }
                        // 현재 컨텐츠가 ToolMessageContent가 아닌 경우
                        // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                        else
                        {
                            await foreach (var rr in OnStreamingReceive(res, new StreamingContentCompletedResponse
                            {
                                Index = index,
                            }))
                            {
                                yield return rr;
                            }
                            current = (index + 1, new ToolMessageContent
                            {
                                IsApproved = request.Tools?.TryGet(func.Function?.Name!, out var t) != true || t?.RequiresApproval == false,
                                Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                                Name = func.Function?.Name ?? string.Empty,
                                Input = func.Function?.Arguments,
                            });
                            currentToolIndex = funcIndex;
                            await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                            {
                                Index = current.Value.Item1,
                                Content = current.Value.Item2
                            }))
                            {
                                yield return rr;
                            }
                        }
                    }
                    // 처음 생성되는 툴 메시지의 경우
                    else
                    {
                        current = (0, new ToolMessageContent
                        {
                            IsApproved = request.Tools?.TryGet(func.Function?.Name!, out var t) != true || t?.RequiresApproval == false,
                            Id = func.Id ?? $"tool_{Guid.NewGuid().ToShort()}",
                            Name = func.Function?.Name ?? string.Empty,
                            Input = func.Function?.Arguments,
                        });
                        currentToolIndex = funcIndex;
                        await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        }))
                        {
                            yield return rr;
                        }
                    }
                }
            }

            // 텍스트 생성
            var text = delta.Content;
            if (text != null)
            {
                // 이어서 생성되는 텍스트 메시지의 경우
                if (current.HasValue)
                {
                    var (index, content) = current.Value;
                    // 현재 컨텐츠가 TextMessageContent인 경우
                    if (content is TextMessageContent textContent)
                    {
                        await foreach (var rr in OnStreamingReceive(res, new StreamingContentDeltaResponse
                        {
                            Index = index,
                            Delta = new TextDeltaContent
                            {
                                Value = text
                            }
                        }))
                        {
                            yield return rr;
                        }
                    }
                    // 현재 컨텐츠가 TextMessageContent가 아닌 경우 (thinking → text 전환 등)
                    // 이전 컨텐츠 종료 및 새 컨텐츠 시작
                    else
                    {
                        await foreach (var rr in OnStreamingReceive(res, new StreamingContentCompletedResponse
                        {
                            Index = index,
                        }))
                        {
                            yield return rr;
                        }
                        current = (index + 1, new TextMessageContent { Value = text });
                        await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                        {
                            Index = current.Value.Item1,
                            Content = current.Value.Item2
                        }))
                        {
                            yield return rr;
                        }
                    }
                }
                // 처음 생성되는 텍스트 메시지의 경우
                else
                {
                    current = (0, new TextMessageContent { Value = text });
                    await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                    {
                        Index = current.Value.Item1,
                        Content = current.Value.Item2
                    }))
                    {
                        yield return rr;
                    }
                }
            }
        }

        // 남아 있는 컨텐츠 처리
        if (current.HasValue)
        {
            await foreach (var rr in OnStreamingReceive(lastChunk, new StreamingContentCompletedResponse
            {
                Index = current.Value.Item1,
            }))
            {
                yield return rr;
            }
        }

        // 종료
        await foreach (var rr in OnStreamingReceive(lastChunk, new StreamingMessageDoneResponse
        {
            DoneReason = reason,
            TokenUsage = usage,
            Id = id,
            Model = model,
            Timestamp = DateTime.UtcNow,
        }))
        {
            yield return rr;
        }
    }

    /// <summary>
    /// OpenAI API 클라이언트에 요청을 보내기 직전에 호출됩니다.
    /// </summary>
    /// <param name="source">
    /// 변환 전의 공용 요청 객체입니다.
    /// </param>
    /// <param name="request">
    /// 공용 요청에서 변환된 OpenAI ChatCompletion 요청 객체입니다.
    /// </param>
    /// <returns>수정된 ChatCompletion 요청 객체</returns>
    protected virtual ChatCompletionRequest OnBeforeSend(
        MessageGenerationRequest source,
        ChatCompletionRequest request)
        => request;

    /// <summary>
    /// OpenAI API 응답을 공용 응답으로 변환한 후, 반환 직전에 호출됩니다.
    /// </summary>
    /// <param name="source">
    /// OpenAI API에서 받은 원본 ChatCompletion 응답 객체입니다.
    /// </param>
    /// <param name="response">
    /// 원본 응답에서 변환된 공용 응답 객체입니다.
    /// </param>
    /// <returns>수정된 공용 응답 객체</returns>
    protected virtual MessageResponse OnAfterReceive(
        ChatCompletionResponse source,
        MessageResponse response)
        => response;

    /// <summary>
    /// 스트리밍 중 각 청크가 공용 스트리밍 응답으로 변환된 후, yield 직전에 호출됩니다.
    /// <list type="bullet">
    ///   <item><description>그대로 전달: <c>yield return chunk;</c> (기본 동작)</description></item>
    ///   <item><description>수정 후 전달: chunk를 변경한 뒤 <c>yield return</c></description></item>
    ///   <item><description>무시(필터링): <c>yield return</c> 하지 않음</description></item>
    ///   <item><description>분할: 여러 개를 <c>yield return</c></description></item>
    /// </list>
    /// </summary>
    /// <param name="source">
    /// OpenAI API에서 받은 원본 스트리밍 청크입니다.
    /// 스트림 종료 후 잔여 처리 시에는 마지막 청크가 전달되며, null일 수 있습니다.
    /// </param>
    /// <param name="chunk">
    /// 원본 청크에서 변환된 공용 스트리밍 응답입니다.
    /// </param>
    /// <returns>최종적으로 소비자에게 전달될 스트리밍 응답들</returns>
    protected virtual async IAsyncEnumerable<StreamingMessageResponse> OnStreamingReceive(
        StreamingChatCompletionResponse? source,
        StreamingMessageResponse chunk)
    {
        yield return chunk;
    }
}
