using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Responses;
using System.Runtime.CompilerServices;
using OpenAIStreamingContentAddedResponse = IronHive.Providers.OpenAI.Payloads.Responses.StreamingContentAddedResponse;
using StreamingContentAddedResponse = IronHive.Abstractions.Messages.StreamingContentAddedResponse;

namespace IronHive.Providers.OpenAI;

/// <inheritdoc />
public class OpenAIResponseMessageGenerator : IMessageGenerator
{
    private readonly OpenAIResponsesClient _client;

    public OpenAIResponseMessageGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIResponseMessageGenerator(OpenAIConfig config)
    {
        _client = new OpenAIResponsesClient(config);
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
        var req = OnBeforeSend(request, request.ToOpenAI());
        var res = await _client.PostResponsesAsync(req, cancellationToken);

        var content = new List<MessageContent>();
        foreach (var item in res.Output)
        {
            if (item is ResponsesMessageItem mi)
            {
                foreach (var c in mi.Content)
                {
                    if (c is ResponsesOutputTextContent tc)
                    {
                        content.Add(new TextMessageContent
                        {
                            Value = tc.Text
                        });
                    }
                    else
                    {
                        // Not Expected type
                    }
                }
            }
            else if (item is ResponsesReasoningItem ri)
            {
                content.Add(new ThinkingMessageContent
                {
                    Format = ThinkingFormat.Summary,
                    Signature = ri.EncryptedContent,
                    Value = ri.Summary?.Count > 0
                        ? string.Join("\n---\n", ri.Summary.Select(s => s.Text.Trim()))
                        : string.Empty
                });
            }
            else if (item is ResponsesFunctionToolCallItem fti)
            {
                content.Add(new ToolMessageContent
                {
                    Id = fti.CallId,
                    Name = fti.Name,
                    Input = fti.Arguments,
                    IsApproved = request.Tools?.TryGet(fti.Name, out var t) != true || t?.RequiresApproval == false
                });
            }
            else
            {
                // Don't know output type
            }
        }

        var reason = MessageDoneReason.EndTurn;
        if (res.IncompleteDetails?.Reason != null)
        {
            reason = res.IncompleteDetails.Reason switch
            {
                "max_output_tokens" => MessageDoneReason.MaxTokens,
                _ => MessageDoneReason.Unknown,
            };
        }
        if (content.OfType<ToolMessageContent>().Any())
        {
            reason = MessageDoneReason.ToolCall;
        }

        return OnAfterReceive(res, new MessageResponse
        {
            Id = res.Id ?? Guid.NewGuid().ToString(),
            DoneReason = reason,
            Message = new AssistantMessage
            {
                Id = res.Id ?? Guid.NewGuid().ToString(),
                Model = res.Model,
                Content = content,
            },
            TokenUsage = new MessageTokenUsage
            {
                InputTokens = res.Usage.InputTokens,
                OutputTokens = res.Usage.OutputTokens
            },
            Timestamp = res.CreatedAt
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = OnBeforeSend(request, request.ToOpenAI());

        int pIndex = 0; // 컨텐츠 파트 인덱스(배열 컨텐츠 추적)
        var reason = MessageDoneReason.EndTurn;

        await foreach (var res in _client.PostStreamingResponsesAsync(req, cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (res is StreamingCreatedResponse scr)
            {
                await foreach (var rr in OnStreamingReceive(res, new StreamingMessageBeginResponse
                {
                    Id = scr.Response.Id,
                }))
                {
                    yield return rr;
                }
            }
            // 2. 컨텐츠 생성 시작(추론, 툴)
            else if (res is StreamingOutputAddedResponse oar)
            {
                // 추론 생성
                if (oar.Item is ResponsesReasoningItem ri)
                {
                    await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                    {
                        Index = oar.OutputIndex,
                        Content = new ThinkingMessageContent
                        {
                            Format = ThinkingFormat.Summary,
                            Signature = ri.EncryptedContent,
                            Value = ri.Summary?.Count > 0
                                ? string.Join("\n---\n", ri.Summary.Select(s => s.Text.Trim()))
                                : string.Empty
                        }
                    }))
                    {
                        yield return rr;
                    }
                }
                // 툴 호출 생성
                else if (oar.Item is ResponsesFunctionToolCallItem tci)
                {
                    reason = MessageDoneReason.ToolCall; // 툴 호출 설정
                    await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                    {
                        Index = oar.OutputIndex,
                        Content = new ToolMessageContent
                        {
                            Id = tci.CallId,
                            Name = tci.Name,
                            Input = tci.Arguments,
                            IsApproved = request.Tools?.TryGet(tci.Name, out var t) != true || t?.RequiresApproval == false
                        }
                    }))
                    {
                        yield return rr;
                    }
                }
                else
                {
                    // Passing Other Items
                }
            }
            // 2. 컨텐츠 생성 시작(텍스트)
            else if (res is OpenAIStreamingContentAddedResponse car)
            {
                if (car.Part is ResponsesTextItemPart tip)
                {
                    await foreach (var rr in OnStreamingReceive(res, new StreamingContentAddedResponse
                    {
                        Index = car.OutputIndex,
                        Content = new TextMessageContent
                        {
                            Value = tip.Text
                        }
                    }))
                    {
                        yield return rr;
                    }
                }
                else
                {
                    // Passing Other Items
                }
            }
            // 3. 컨텐츠 생성(텍스트)
            else if (res is StreamingTextDeltaResponse td)
            {
                var isAdded = pIndex != td.ContentIndex;
                if (isAdded) pIndex = td.ContentIndex.GetValueOrDefault(0);

                await foreach (var rr in OnStreamingReceive(res, new StreamingContentDeltaResponse
                {
                    Index = td.OutputIndex,
                    Delta = new TextDeltaContent
                    {
                        Value = isAdded ? $"\n---\n{td.Delta}" : td.Delta
                    },
                }))
                {
                    yield return rr;
                }
            }
            // 3. 컨텐츠 생성(추론)
            else if (res is StreamingReasoningSummaryDeltaResponse rsd)
            {
                var isAdded = pIndex != rsd.SummaryIndex;
                if (isAdded) pIndex = rsd.SummaryIndex;

                await foreach (var rr in OnStreamingReceive(res, new StreamingContentDeltaResponse
                {
                    Index = rsd.OutputIndex,
                    Delta = new ThinkingDeltaContent
                    {
                        Data = isAdded ? $"\n---\n{rsd.Delta}" : rsd.Delta
                    },
                }))
                {
                    yield return rr;
                }
            }
            // 3. 컨텐츠 생성(툴)
            else if (res is StreamingFunctionToolDeltaResponse tdr)
            {
                await foreach (var rr in OnStreamingReceive(res, new StreamingContentDeltaResponse
                {
                    Index = tdr.OutputIndex,
                    Delta = new ToolDeltaContent
                    {
                        Input = tdr.Delta
                    },
                }))
                {
                    yield return rr;
                }
            }
            // 4. 컨텐츠 생성 종료
            else if (res is StreamingOutputDoneResponse odr)
            {
                // 추론 컨텐츠 업데이트
                if (odr.Item is ResponsesReasoningItem ri)
                {
                    await foreach (var rr in OnStreamingReceive(res, new StreamingContentUpdatedResponse
                    {
                        Index = odr.OutputIndex,
                        Updated = new ThinkingUpdatedContent
                        {
                            Signature = ri.EncryptedContent ?? string.Empty
                        }
                    }))
                    {
                        yield return rr;
                    }
                }

                pIndex = 0; // 파트 리셋
                await foreach (var rr in OnStreamingReceive(res, new StreamingContentCompletedResponse
                {
                    Index = odr.OutputIndex
                }))
                {
                    yield return rr;
                }
            }
            // 5. 메시지 종료
            else if (res is StreamingCompletedResponse sdr)
            {
                await foreach (var rr in OnStreamingReceive(res, new StreamingMessageDoneResponse
                {
                    DoneReason = reason,
                    Id = sdr.Response.Id,
                    Model = sdr.Response.Model,
                    TokenUsage = new MessageTokenUsage
                    {
                        InputTokens = sdr.Response.Usage.InputTokens,
                        OutputTokens = sdr.Response.Usage.OutputTokens
                    },
                    Timestamp = sdr.Response.CreatedAt
                }))
                {
                    yield return rr;
                }
            }
            // 5. 메시지 비정상 종료
            else if (res is StreamingIncompleteResponse sir)
            {
                reason = sir.Response.IncompleteDetails?.Reason switch
                {
                    "max_output_tokens" => MessageDoneReason.MaxTokens,
                    _ => MessageDoneReason.Unknown,
                };
                await foreach (var rr in OnStreamingReceive(res, new StreamingMessageDoneResponse
                {
                    DoneReason = reason,
                    Id = sir.Response.Id,
                    Model = sir.Response.Model,
                    TokenUsage = new MessageTokenUsage
                    {
                        InputTokens = sir.Response.Usage.InputTokens,
                        OutputTokens = sir.Response.Usage.OutputTokens
                    },
                    Timestamp = sir.Response.CreatedAt
                }))
                {
                    yield return rr;
                }
            }
            // 이외 이벤트 건너뜀(or 디버깅)
            else
            {
                // Passing Other Streaming Events
                //Debug.WriteLine(JsonSerializer.Serialize(res));
            }
        }
    }

    /// <summary>
    /// 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    /// <param name="source">
    /// 변환 전의 공용 요청 객체입니다.
    /// </param>
    /// <param name="request">
    /// 공용 요청에서 변환된 OpenAI Responses 요청 객체입니다.
    /// </param>
    /// <returns>수정된 Responses 요청 객체</returns>
    protected virtual ResponsesRequest OnBeforeSend(
        MessageGenerationRequest source,
        ResponsesRequest request)
        => request;

    /// <summary>
    /// 응답을 받은 후 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    /// <param name="source">
    /// OpenAI API에서 받은 원본 Responses 응답 객체입니다.
    /// </param>
    /// <param name="response">
    /// 원본 응답에서 변환된 공용 응답 객체입니다.
    /// </param>
    /// <returns>수정된 공용 응답 객체</returns>
    protected virtual MessageResponse OnAfterReceive(
        ResponsesResponse source,
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
    /// </param>
    /// <param name="chunk">
    /// 원본 청크에서 변환된 공용 스트리밍 응답입니다.
    /// </param>
    /// <returns>최종적으로 소비자에게 전달될 스트리밍 응답들</returns>
    protected virtual async IAsyncEnumerable<StreamingMessageResponse> OnStreamingReceive(
        StreamingResponsesResponse source,
        StreamingMessageResponse chunk)
    {
        yield return chunk;
    }
}
