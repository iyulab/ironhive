using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.Assistant;

public interface IRaggleAssistant
{
    Task<ChatCompletionResponse> ChatCompletionAsync(MessageCollection messages);

    IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(MessageCollection messages);
}
