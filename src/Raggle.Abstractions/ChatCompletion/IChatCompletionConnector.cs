using Raggle.Abstractions.ChatCompletion.Messages;

namespace Raggle.Abstractions.ChatCompletion;

public interface IChatCompletionConnector
{
    /// <summary>
    /// Retrieves a list of available chat completion models.
    /// </summary>
    Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion message based on the provided request.
    /// </summary>
    Task<ChatCompletionResponse<IMessage>> GenerateMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion streaming message based on the provided request.
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse<IMessageContent>> GenerateStreamingMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
