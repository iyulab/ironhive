using Raggle.Abstractions;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using System.Runtime.CompilerServices;

namespace Raggle.Core.ChatCompletion;

public class ChatCompletionService : IChatCompletionService
{
    private readonly IReadOnlyDictionary<string, IChatCompletionConnector> _connectors;
    private readonly ITextParser<(string, string)> _parser;

    public ChatCompletionService(
        IHiveServiceRegistry registry,
        ITextParser<(string, string)> parser)
    {
        _connectors = registry.GetKeyedServices<IChatCompletionConnector>();
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<ChatCompletionModel>();
        foreach (var (key, connector) in _connectors)
        {
            var serviceModels = await connector.GetModelsAsync(cancellationToken);

            models.AddRange(serviceModels.Select(x => new ChatCompletionModel
            {
                Model = _parser.Stringify((key, x.Model)),
            }));
        }
        return models;
    }

    /// <inheritdoc />
    public async Task<ChatCompletionResponse<IMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(request.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
        {
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        }

        request.Model = model;
        return await connector.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(request.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
        {
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        }

        request.Model = model;
        await foreach(var res in connector.GenerateStreamingMessageAsync(request, cancellationToken))
        {
            yield return res;
        }
    }
}
