﻿using System.Runtime.CompilerServices;
using System.Text.Json;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Providers.Ollama.Chat;

namespace IronHive.Providers.Ollama;

public class OllamaMessageGenerator : IMessageGenerator
{
    private readonly OllamaChatClient _client;

    public OllamaMessageGenerator(OllamaConfig? config = null)
    {
        _client = new OllamaChatClient(config);
    }

    public OllamaMessageGenerator(string baseUrl)
    {
        _client = new OllamaChatClient(baseUrl);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var req = request.ToOllama();
        var res = await _client.PostChatAsync(req, cancellationToken);

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
                    Id = $"tool_{Guid.NewGuid().ToShort()}",
                    Name = t.Function?.Name ?? string.Empty,
                    Input = JsonSerializer.Serialize(t.Function?.Arguments)
                });
            }
        }

        return new MessageResponse
        {
            DoneReason = res.DoneReason switch
            {
                DoneReason.Stop => MessageDoneReason.EndTurn,
                _ => null
            },
            Message = message,
            TokenUsage = null,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var req = request.ToOllama();

        await foreach (var res in _client.PostSteamingChatAsync(req, cancellationToken))
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
                yield return new StreamingMessageDoneResponse
                {
                    Id = Guid.NewGuid().ToShort(),
                    Model = res.Model,
                    DoneReason = res.DoneReason switch
                    {
                        DoneReason.Stop => MessageDoneReason.EndTurn,
                        _ => null
                    },
                    Timestamp = DateTime.UtcNow,
                };
            }
        }
    }
}
