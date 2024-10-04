using Raggle.Abstractions.Models;

namespace Raggle.Abstractions.Engines;

public interface IChatEngine
{
    Task<IEnumerable<ChatModel>> GetChatCompletionModelsAsync();

    Task<ChatResponse> ChatCompletionAsync(ChatHistory history, ChatOptions options);

    IAsyncEnumerable<IStreamingChatResponse> StreamingChatCompletionAsync(ChatHistory history, ChatOptions options);
}
