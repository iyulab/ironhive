using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.ChatCompletion;

public interface IChatCompletionService
{
    /// <summary>
    /// Retrieves a list of available chat completion models by service provider.
    /// </summary>
    Task<IEnumerable<ChatCompletionModel>> GetModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a chat completion models information.
    /// </summary>
    Task<ChatCompletionModel> GetModelAsync(
        string provider,
        string model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion message based on the provided request.
    /// </summary>
    Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        UserMessage message,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion streaming message based on the provided request.
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        UserMessage message,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion message based on the provided request.
    /// </summary>
    Task<ChatCompletionResponse<AssistantMessage>> GenerateMessageAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion streaming message based on the provided request.
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse<IAssistantContent>> GenerateStreamingMessageAsync(
        MessageCollection messages, 
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);
}
