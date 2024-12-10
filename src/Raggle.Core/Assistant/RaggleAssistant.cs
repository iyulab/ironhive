using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.Assistant;

internal class RaggleAssistant : IRaggleAssistant
{
    private readonly IChatCompletionService _service;

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public required string Provider { get; set; }

    public required string Model { get; set; }

    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }

    public int? TopK { get; set; }

    public float? TopP { get; set; }

    public string[]? StopSequences { get; set; }

    public FunctionToolCollection? Tools { get; set; }

    public RaggleAssistant(IServiceProvider services)
    {
        _service = services.GetRequiredKeyedService<IChatCompletionService>(Provider);
    }

    public Task<ChatCompletionResponse> ChatCompletionAsync(MessageCollection messages)
    {
        var request = BuildRequest(messages);
        return _service.ChatCompletionAsync(request);
    }

    public IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(MessageCollection messages)
    {
        var request = BuildRequest(messages);
        return _service.StreamingChatCompletionAsync(request);
    }

    private ChatCompletionRequest BuildRequest(MessageCollection messages)
    {
        return new ChatCompletionRequest
        {
            Model = Model,
            System = Instruction,
            MaxTokens = MaxTokens,
            Messages = messages,
            Temperature = Temperature,
            StopSequences = StopSequences,
            Tools = Tools,
            TopK = TopK,
            TopP = TopP
        };
    }
}
