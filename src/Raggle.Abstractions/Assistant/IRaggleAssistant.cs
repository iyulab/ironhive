using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.Assistant;

public interface IRaggleAssistant
{
    Task<ChatCompletionResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);
}
