using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Driver.Ollama.ChatCompletion;
using Raggle.Driver.Ollama.ChatCompletion.Models;
using Raggle.Driver.Ollama.Configurations;
using System.Runtime.CompilerServices;

namespace Raggle.Driver.Ollama;

public class OllamaChatCompletionService : IChatCompletionService
{
    private readonly OllamaChatCompletionClient _client;

    public OllamaChatCompletionService(OllamaConfig? config = null)
    {
        _client = new OllamaChatCompletionClient(config);
    }

    public OllamaChatCompletionService(string endPoint)
    {
        _client = new OllamaChatCompletionClient(endPoint);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetChatModelsAsync(cancellationToken);
        return models.Select(m => new ChatCompletionModel
        {
            Model = m.Name,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
            Owner = null
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse> ChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var _request = request.ToOllama();
        var response = await _client.PostChatAsync(_request, cancellationToken);
        var content = new MessageContentCollection();
        content.AddText(response.Message?.Content);
        return new ChatCompletionResponse
        {
            Model = response.Model,
            EndReason = response.DoneReason switch
            {
                DoneReason.Stop => ChatCompletionEndReason.EndTurn,
                _ => null
            },
            Message = new Message
            {
                Role = MessageRole.Assistant,
                Content = content,
                TimeStamp = DateTime.UtcNow,
            },
            TokenUsage = null,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = request.ToOllama();
        await foreach (var res in _client.PostSteamingChatAsync(_request, cancellationToken))
        {
            yield return new ChatCompletionStreamingResponse
            {
                Content = new TextContent
                {
                    Text = res.Message?.Content
                },
            };
        }
    }
}
