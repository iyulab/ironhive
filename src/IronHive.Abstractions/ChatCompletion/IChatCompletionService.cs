using IronHive.Abstractions.ChatCompletion.Messages;

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
    Task<Message> ExecuteAsync(
        MessageCollection messages,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion streaming message based on the provided request.
    /// </summary>
    IAsyncEnumerable<IMessageContent> ExecuteStreamingAsync(
        MessageCollection messages, 
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default);
}
