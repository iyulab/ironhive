using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Connectors.Ollama.Base;
using Raggle.Connectors.Ollama.ChatCompletion;
using Raggle.Connectors.Ollama.Configurations;
using Raggle.Connectors.Ollama.Extensions;
using System.Runtime.CompilerServices;
using Message = Raggle.Abstractions.ChatCompletion.Messages.Message;
using OllamaMessage = Raggle.Connectors.Ollama.ChatCompletion.Message;

namespace Raggle.Connectors.Ollama;

public class OllamaChatCompletionConnector : IChatCompletionConnector
{
    private readonly OllamaChatCompletionClient _client;

    public OllamaChatCompletionConnector(OllamaConfig? config = null)
    {
        _client = new OllamaChatCompletionClient(config);
    }

    public OllamaChatCompletionConnector(string baseUrl)
    {
        _client = new OllamaChatCompletionClient(baseUrl);
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
    public async Task<ChatCompletionResult<Message>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages, options);
        var response = await _client.PostChatAsync(request, cancellationToken);

        var content = new MessageContentCollection();
        content.AddText(response.Message?.Content);

        return new ChatCompletionResult<Message>
        {
            EndReason = response.DoneReason switch
            {
                DoneReason.Stop => EndReason.EndTurn,
                _ => null
            },
            TokenUsage = null,
            Data = new Message
            {

                Content = content
            },
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResult<IMessageContent>> GenerateStreamingMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages, options);

        await foreach (var res in _client.PostSteamingChatAsync(request, cancellationToken))
        {
            yield return new ChatCompletionResult<IMessageContent>
            {
                Data = new TextContent
                {
                    Value = res.Message?.Content
                },
            };
        }
    }

    private static ChatRequest BuildRequest(MessageCollection messages, ChatCompletionOptions options)
    {
        var request = new ChatRequest
        {
            Model = options.Model,
            Messages = messages.ToOllama(options?.System),
            Options = new ModelOptions
            {
                NumPredict = options?.MaxTokens,
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                TopK = options?.TopK,
                Stop = options?.StopSequences != null
                    ? string.Join(" ", options.StopSequences)
                    : null,
            },
        };

        // 동작하지 않는 모델 존재함
        //if (options?.Tools != null && options.Tools.Count > 0)
        //{
        //    request.Tools = options.Tools.Select(t =>
        //    {
        //        return new Tool
        //        {
        //            Function = new FunctionTool
        //            {
        //                Name = t.Name,
        //                Description = t.Description,
        //                Parameters = new ParametersSchema
        //                {
        //                    Properties = t.Parameters,
        //                    Required = t.Required,
        //                }
        //            }
        //        };
        //    });
        //}

        return request;
    }
}
