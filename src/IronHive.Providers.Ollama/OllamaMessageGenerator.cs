using IronHive.Abstractions.Message;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.Ollama.Clients;
using IronHive.Providers.Ollama.Payloads.Chat;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IronHive.Providers.Ollama;

/// <inheritdoc />
public class OllamaMessageGenerator : IMessageGenerator
{
    private readonly OllamaChatClient _client;

    public OllamaMessageGenerator(string baseUrl)
        : this(new OllamaConfig { BaseUrl = baseUrl })
    { }

    public OllamaMessageGenerator(OllamaConfig? config = null)
    {
        _client = new OllamaChatClient(config);
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
        var req = request.ToOllama();
        var res = await _client.PostChatAsync(req, cancellationToken).ConfigureAwait(false);

        var message = new AssistantMessage
        {
            Id = Guid.NewGuid().ToShort()
        };

        // 텍스트 생성
        var text = res.Message?.Content;
        if (text != null)
        {
            message.Content.Add(new TextMessageContent
            {
                Value = text
            });
        }

        // 도구 호출
        var tools = res.Message?.ToolCalls;
        if (tools != null)
        {
            foreach (var t in tools)
            {
                message.Content.Add(new ToolMessageContent
                {
                    IsApproved = request.Tools?.TryGet(t.Function?.Name!, out var tool) != true || !tool.RequiresApproval,
                    Id = $"tool_{Guid.NewGuid().ToShort()}",
                    Name = t.Function?.Name ?? string.Empty,
                    Input = JsonSerializer.Serialize(t.Function?.Arguments)
                });
            }
        }

        // 토큰 사용량 추출
        MessageTokenUsage? tokenUsage = null;
        if (res.PromptEvalCount.HasValue || res.EvalCount.HasValue)
        {
            tokenUsage = new MessageTokenUsage
            {
                InputTokens = res.PromptEvalCount ?? 0,
                OutputTokens = res.EvalCount ?? 0
            };
        }

        return new MessageResponse
        {
            Id = Guid.NewGuid().ToShort(),
            DoneReason = res.DoneReason switch
            {
                DoneReason.Stop => MessageDoneReason.EndTurn,
                _ => null
            },
            Message = message,
            TokenUsage = tokenUsage,
            Timestamp = res.CreatedAt ?? DateTime.UtcNow,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToOllama();

        await foreach (var res in _client.PostSteamingChatAsync(req, cancellationToken).ConfigureAwait(false))
        {
            // 텍스트 생성
            var text = res.Message?.Content;
            if (text != null)
            {
                yield return new StreamingContentDeltaResponse
                {
                    Index = 0,
                    Delta = new TextDeltaContent
                    {
                        Value = text
                    }
                };
            }

            // 종료 메시지
            if (res.DoneReason != null)
            {
                // 토큰 사용량 추출
                MessageTokenUsage? tokenUsage = null;
                if (res.PromptEvalCount.HasValue || res.EvalCount.HasValue)
                {
                    tokenUsage = new MessageTokenUsage
                    {
                        InputTokens = res.PromptEvalCount ?? 0,
                        OutputTokens = res.EvalCount ?? 0
                    };
                }

                yield return new StreamingMessageDoneResponse
                {
                    Id = Guid.NewGuid().ToShort(),
                    Model = res.Model,
                    DoneReason = res.DoneReason switch
                    {
                        DoneReason.Stop => MessageDoneReason.EndTurn,
                        _ => null
                    },
                    TokenUsage = tokenUsage,
                    Timestamp = DateTime.UtcNow,
                };
            }
        }
    }
}
