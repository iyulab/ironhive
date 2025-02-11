using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.Assistant;

public interface IRaggleAssistant
{
    Task<MessageResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);
}
