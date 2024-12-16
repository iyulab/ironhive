using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.Assistant;

public interface IRaggleAssistant
{
    Task<ChatCompletionResponse> ChatCompletionAsync(
        MessageCollection messages,
        AssistantOptions? options,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        MessageCollection messages,
        AssistantOptions? options,
        CancellationToken cancellationToken = default);
}
