using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.Assistant;

internal class BasicRaggleAssistant : IRaggleAssistant
{
    private readonly IChatCompletionService _service;

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
        _service = provider.GetRequiredKeyedService<IChatCompletionService>(Service);
    }

    public Task<ChatCompletionResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        return _service.ChatCompletionAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        return _service.StreamingChatCompletionAsync(request, cancellationToken);
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
