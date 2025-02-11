namespace Raggle.Abstractions.AI;

public interface IChatCompletionService
{
    /// <summary>
    /// Retrieves a list of available chat completion models.
    /// </summary>
    Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion based on the provided request.
    /// </summary>
    Task<MessageResponse> ChatCompletionAsync(
        MessageContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams chat completion responses based on the provided request.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> StreamingChatCompletionAsync(
        MessageContext context,
        CancellationToken cancellationToken = default);
}
