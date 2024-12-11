namespace Raggle.Abstractions.AI;

public interface IChatCompletionService
{
    /// <summary>
    /// Retrieves a list of available chat completion models.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable of <see cref="ChatCompletionModel"/>.
    /// </returns>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion based on the provided request.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="ChatCompletionResponse"/>.
    /// </returns>
    Task<ChatCompletionResponse> ChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams chat completion responses based on the provided request.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="ChatCompletionStreamingResponse"/>.
    /// </returns>
    IAsyncEnumerable<ChatCompletionStreamingResponse> StreamingChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
