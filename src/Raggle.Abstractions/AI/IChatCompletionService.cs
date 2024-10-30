using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public interface IChatCompletionService
{
    Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync();

    Task<ChatCompletionResponse> ChatCompletionAsync(ChatHistory history, ChatCompletionOptions options);

    IAsyncEnumerable<IStreamingChatCompletionResponse> StreamingChatCompletionAsync(ChatHistory history, ChatCompletionOptions options);
}
