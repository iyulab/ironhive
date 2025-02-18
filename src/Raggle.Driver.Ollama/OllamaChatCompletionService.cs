using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Driver.Ollama.Base;
using Raggle.Driver.Ollama.ChatCompletion;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.Ollama.Extensions;
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
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new ChatCompletionModel
        {
            Model = m.Name,
            Owner = null,
            CreatedAt = null,
            ModifiedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<IEnumerable<IMessageContent>>> GenerateMessageAsync(
        ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOllama(request);
        var response = await _client.PostChatAsync(_request, cancellationToken);

        var content = new MessageContentCollection();
        content.AddText(response.Message?.Content);

        return new ChatCompletionResponse<IEnumerable<IMessageContent>>
        {
            EndReason = response.DoneReason switch
            {
                DoneReason.Stop => ChatCompletionEndReason.EndTurn,
                _ => null
            },
            TokenUsage = null,
            Content = content,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var _request = ConvertToOllama(request);

        await foreach (var res in _client.PostSteamingChatAsync(_request, cancellationToken))
        {
            yield return new ChatCompletionResponse<IMessageContent>
            {
                Content = new TextContent
                {
                    Text = res.Message?.Content
                },
            };
        }
    }

    private static ChatRequest ConvertToOllama(ChatCompletionRequest request)
    {
        var _request = new ChatRequest
        {
            Model = request.Model,
            Messages = request.Messages.ToOllama(request?.System),
            Options = new ModelOptions
            {
                NumPredict = request?.MaxTokens,
                Temperature = request?.Temperature,
                TopP = request?.TopP,
                TopK = request?.TopK,
                Stop = request?.StopSequences != null
                    ? string.Join(" ", request.StopSequences)
                    : null,
            },
        };

        // 동작하지 않는 모델 존재함
        //if (request?.Tools != null && request.Tools.Count > 0)
        //{
        //    _request.Tools = request.Tools.Select(t =>
        //    {
        //        return new Tool
        //        {
        //            Function = new FunctionTool
        //            {
        //                Name = t.Name,
        //                Description = t.Description,
        //                Parameters = new ParametersSchema
        //                {
        //                    Properties = t.Properties,
        //                    Required = t.Required,
        //                }
        //            }
        //        };
        //    });
        //}

        return _request;
    }
}
