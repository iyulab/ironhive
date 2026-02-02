using System.Runtime.CompilerServices;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Responses;
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
        var req = request.ToOpenAI();
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
                    Value = string.Join("\n---\n", ri.Summary.Select(s => s.Text.Trim()))
                });
            }
            else if (item is ResponsesFunctionToolCallItem fti)
            {
                content.Add(new ToolMessageContent
                {
                    Id = fti.CallId,
                    Name = fti.Name,
                    Input = fti.Arguments,
                    IsApproved = request.Tools?.TryGet(fti.Name, out var t) != true || !t.RequiresApproval
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

        return new MessageResponse
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
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToOpenAI();

        int pIndex = 0; // 컨텐츠 파트 인덱스(배열 컨텐츠 추적)
        var reason = MessageDoneReason.EndTurn;

        await foreach (var res in _client.PostStreamingResponsesAsync(req, cancellationToken))
        {
            // 1. 메시지 시작 이벤트
            if (res is StreamingCreatedResponse scr)
            {
                yield return new StreamingMessageBeginResponse
                {
                   Id = scr.Response.Id,
                };
            }
            // 2. 컨텐츠 생성 시작(추론, 툴)
            else if (res is StreamingOutputAddedResponse oar)
            {
                // 추론 생성
                if (oar.Item is ResponsesReasoningItem ri)
                {
                    yield return new StreamingContentAddedResponse
                    {
                        Index = oar.OutputIndex,
                        Content = new ThinkingMessageContent
                        {
                            Format = ThinkingFormat.Summary,
                            Signature = ri.EncryptedContent,
                            Value = string.Join("\n---\n", ri.Summary.Select(s => s.Text.Trim()))
                        }
                    };
                }
                // 툴 호출 생성
                else if (oar.Item is ResponsesFunctionToolCallItem tci)
                {
                    reason = MessageDoneReason.ToolCall; // 툴 호출 설정
                    yield return new StreamingContentAddedResponse
                    {
                        Index = oar.OutputIndex,
                        Content = new ToolMessageContent
                        {
                            Id = tci.CallId,
                            Name = tci.Name,
                            Input = tci.Arguments,
                            IsApproved = request.Tools?.TryGet(tci.Name, out var t) != true || !t.RequiresApproval
                        }
                    };
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
                    yield return new StreamingContentAddedResponse
                    {
                        Index = car.OutputIndex,
                        Content = new TextMessageContent
                        {
                            Value = tip.Text
                        }
                    };
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

                yield return new StreamingContentDeltaResponse
                {
                    Index = td.OutputIndex,
                    Delta = new TextDeltaContent
                    {
                        Value = isAdded ? $"\n---\n{td.Delta}" : td.Delta
                    },
                };
            }
            // 3. 컨텐츠 생성(추론)
            else if (res is StreamingReasoningSummaryDeltaResponse rsd)
            {
                var isAdded = pIndex != rsd.SummaryIndex;
                if (isAdded) pIndex = rsd.SummaryIndex;

                yield return new StreamingContentDeltaResponse
                {
                    Index = rsd.OutputIndex,
                    Delta = new ThinkingDeltaContent
                    {
                        Data = isAdded ? $"\n---\n{rsd.Delta}" : rsd.Delta
                    },
                };
            }
            // 3. 컨텐츠 생성(툴)
            else if (res is StreamingFunctionToolDeltaResponse tdr)
            {
                yield return new StreamingContentDeltaResponse
                {
                    Index = tdr.OutputIndex,
                    Delta = new ToolDeltaContent
                    {
                        Input = tdr.Delta
                    },
                };
            }
            // 4. 컨텐츠 생성 종료
            else if (res is StreamingOutputDoneResponse odr)
            {
                // 추론 컨텐츠 업데이트
                if (odr.Item is ResponsesReasoningItem ri)
                {
                    yield return new StreamingContentUpdatedResponse
                    {
                        Index = odr.OutputIndex,
                        Updated = new ThinkingUpdatedContent
                        {
                            Signature = ri.EncryptedContent ?? string.Empty
                        }
                    };
                }

                pIndex = 0; // 파트 리셋
                yield return new StreamingContentCompletedResponse
                {
                    Index = odr.OutputIndex
                };
            }
            // 5. 메시지 종료
            else if (res is StreamingCompletedResponse sdr)
            {
                yield return new StreamingMessageDoneResponse
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
                };
            }
            // 5. 메시지 비정상 종료
            else if (res is StreamingIncompleteResponse sir)
            {
                reason = sir.Response.IncompleteDetails?.Reason switch
                {
                    "max_output_tokens" => MessageDoneReason.MaxTokens,
                    _ => MessageDoneReason.Unknown,
                };
                yield return new StreamingMessageDoneResponse
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
                };
            }
            // 이외 이벤트 건너뜀(or 디버깅)
            else
            {
                // Passing Other Streaming Events
                //Debug.WriteLine(JsonSerializer.Serialize(res));
            }
        }
    }
}
