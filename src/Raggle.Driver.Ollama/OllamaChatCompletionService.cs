using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Driver.Ollama.Base;
using Raggle.Driver.Ollama.ChatCompletion;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.Ollama.Extensions;
using System.Runtime.CompilerServices;
using Message = Raggle.Abstractions.Messages.Message;
using MessageRole = Raggle.Abstractions.Messages.MessageRole;

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
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task<MessageResponse> ChatCompletionAsync(
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        var request = ConvertToRequest(context);
        var response = await _client.PostChatAsync(request, cancellationToken);

        var content = new MessageContentCollection();
        content.AddText(response.Message?.Content);

        return new MessageResponse
        {
            Model = response.Model,
            EndReason = response.DoneReason switch
            {
                DoneReason.Stop => MessageEndReason.EndTurn,
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
    public async IAsyncEnumerable<StreamingMessageResponse> StreamingChatCompletionAsync(
        MessageContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = ConvertToRequest(context);

        await foreach (var res in _client.PostSteamingChatAsync(request, cancellationToken))
        {
            yield return new StreamingMessageResponse
            {
                Content = new TextContent
                {
                    Text = res.Message?.Content
                },
            };
        }
    }

    private static ChatRequest ConvertToRequest(MessageContext context)
    {
        var request = new ChatRequest
        {
            Model = context.Model,
            Messages = context.Messages.ToOllama(context.MessagesOptions?.System).ToArray(),
            Options = new ModelOptions
            {
                NumPredict = context.Parameters?.MaxTokens,
                Temperature = context.Parameters?.Temperature,
                TopP = context.Parameters?.TopP,
                TopK = context.Parameters?.TopK,
                Stop = context.Parameters?.StopSequences != null 
                    ? string.Join(" ", context.Parameters.StopSequences) 
                    : null,
            },
        };

        //if (context.Tools != null && context.Tools.Count > 0)
        //{
        //    request.Tools = context.Tools.Select(t =>
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
        //                    Required = t.Required?.ToArray(),
        //                }
        //            }
        //        };
        //    }).ToArray();
        //}

        return request;
    }
}
