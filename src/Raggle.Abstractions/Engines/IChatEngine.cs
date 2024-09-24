using Raggle.Abstractions.Models;

namespace Raggle.Abstractions.Engines;

public interface IChatEngine
{
    Task<IEnumerable<ChatModel>> GetChatCompletionModelsAsync();

    Task<ChatResponse> ChatCompletionAsync(ChatSession session, ChatOptions options);

    IAsyncEnumerable<StreamingChatResponse> StreamingChatCompletionAsync(ChatSession session, ChatOptions options);
}
