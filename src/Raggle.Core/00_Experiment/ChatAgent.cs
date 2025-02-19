using Raggle.Abstractions.AI;
using Raggle.Abstractions.Experiment;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.Experiment;

public class ChatAgent : IAgent<MessageCollection, ChatCompletionResponse<IMessage>>
{
    private readonly IChatCompletionService _service;

    public required string Instruction { get; set; }
    public required ChatCompletionParameters Params { get; set; }
    public ToolCollection? Tools { get; set; }

    public ChatAgent(IChatCompletionService service)
    {
        _service = service;
    }

    protected virtual Task<ChatCompletionResponse<IMessage>> InvokeAsync(
        MessageCollection input,
        CancellationToken cancellationToken = default)
    {
        return _service.GenerateMessageAsync(new ChatCompletionRequest
        {
            System = Instruction,
            Messages = input,
            Tools = Tools,
            Model = Params.Model,
            MaxTokens = Params.MaxTokens,
            Temperature = Params.Temperature,
            StopSequences = Params.StopSequences,
            TopK = Params.TopK,
            TopP = Params.TopP
        }, cancellationToken);
    }

    protected virtual IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> InvokeStreamingAsync(
        MessageCollection input,
        CancellationToken cancellationToken = default)
    {
        return _service.GenerateStreamingMessageAsync(new ChatCompletionRequest
        {
            System = Instruction,
            Messages = input,
            Tools = Tools,
            Model = Params.Model,
            MaxTokens = Params.MaxTokens,
            Temperature = Params.Temperature,
            StopSequences = Params.StopSequences,
            TopK = Params.TopK,
            TopP = Params.TopP
        }, cancellationToken);
    }

    Task<ChatCompletionResponse<IMessage>> IAgent<MessageCollection, ChatCompletionResponse<IMessage>>.InvokeAsync(MessageCollection input, CancellationToken cancellationToken)
    {
        return InvokeAsync(input, cancellationToken);
    }
}
