using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.Experimental;

public interface IMessageSession
{
    public enum CondensationStrategy
    {
        None,
        Truncate,
        Summarize
    }

    string? Id { get; set; }
    string? Title { get; set; }
    TokenUsage? TokenUsage { get; set; }
    int MaxToolRetryAttempts { get; set; }
    CondensationStrategy Strategy { get; set; }
    MessageCollection Messages { get; set; }
}

public interface IAgent
{
    string Id { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? Instruction { get; set; }
    ChatCompletionParameters? Parameters { get; set; }

    Task<ChatCompletionResponse<IMessageContent>> InvokeAsync(
        MessageContentCollection messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> InvokeStreamingAsync(
        MessageContentCollection messages,
        CancellationToken cancellationToken = default);
}

public interface IWork<TInput, TOutput>
{
    Task<TOutput?> InvokeAsync(
        TInput? input,
        CancellationToken cancellationToken = default);
}


