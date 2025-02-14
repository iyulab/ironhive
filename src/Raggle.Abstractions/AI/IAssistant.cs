using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public interface IAssistant
{
    Task<MessageResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default);
}
