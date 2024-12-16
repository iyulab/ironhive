using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.Assistant;

internal class RaggleAssistant : IRaggleAssistant
{
    private readonly IServiceProvider _provider;

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Instruction { get; set; }    

    public required AssistantOptions DefaultOptions { get; set; }

    public FunctionToolCollection? Tools { get; set; }

    public RaggleAssistant(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<ChatCompletionResponse> ChatCompletionAsync(
        MessageCollection messages,
        AssistantOptions? options,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultOptions;
        var request = BuildRequest(messages, options);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(options.Provider);
        return service.ChatCompletionAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        MessageCollection messages,
        AssistantOptions? options,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultOptions;
        var request = BuildRequest(messages, options);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(options.Provider);
        return service.StreamingChatCompletionAsync(request, cancellationToken);
    }

    private ChatCompletionRequest BuildRequest(
        MessageCollection messages,
        AssistantOptions options)
    {
        return new ChatCompletionRequest
        {
            Model = options.Model,
            System = Instruction,
            Messages = messages,
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            StopSequences = options.StopSequences,
            TopK = options.TopK,
            TopP = options.TopP,
            Tools = Tools
        };
    }
}
