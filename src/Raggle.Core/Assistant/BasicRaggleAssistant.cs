using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.Assistant;

internal class BasicRaggleAssistant : IRaggleAssistant
{
    private readonly IServiceProvider _provider;

    public required string Service { get; set; }

    public required string Model { get; set; }

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public ExecuteOptions? Options { get; set; }

    public FunctionToolCollection? Tools { get; set; }

    public BasicRaggleAssistant(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<ChatCompletionResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(Service);
        return service.ChatCompletionAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(Service);
        return service.StreamingChatCompletionAsync(request, cancellationToken);
    }

    private ChatCompletionRequest BuildRequest(
        MessageCollection messages)
    {
        return new ChatCompletionRequest
        {
            Model = Model,
            System = Instruction,
            Messages = messages,
            MaxTokens = Options?.MaxTokens,
            Temperature = Options?.Temperature,
            StopSequences = Options?.StopSequences,
            TopK = Options?.TopK,
            TopP = Options?.TopP,
            Tools = Tools
        };
    }
}
