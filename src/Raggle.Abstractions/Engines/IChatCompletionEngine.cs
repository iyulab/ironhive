namespace Raggle.Abstractions.Engines;

public interface IChatCompletionEngine
{
    Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync();
    Task<IEnumerable<ChatCompletionResponse>> ChatCompletionAsync();
    IAsyncEnumerable<StreamingChatCompletionResponse> StreamingChatCompletionAsync();
}

public class ChatCompletionModel
{
    public required string ModelId { get; set; }
}

public class ChatCompletionResponse
{

}

public class StreamingChatCompletionResponse
{

}