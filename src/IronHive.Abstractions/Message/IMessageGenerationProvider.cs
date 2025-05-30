namespace IronHive.Abstractions.Message;

public interface IMessageGenerationProvider
{
    /// <summary>
    /// the provider key of the message generation provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates a chat completion message based on the provided request.
    /// </summary>
    Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion streaming message based on the provided request.
    /// </summary>
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
