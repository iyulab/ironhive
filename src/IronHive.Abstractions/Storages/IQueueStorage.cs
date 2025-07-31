namespace IronHive.Abstractions.Storages;

/// <summary>
/// Represents a storage for queues
/// </summary>
public interface IQueueStorage<T> : IDisposable
{
    /// <summary>
    /// Adds a message to the queue
    /// </summary>
    Task EnqueueAsync(
        T message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to dequeue a message and marks it as pending.
    /// The message must be acknowledged to be removed permanently.
    /// </summary>
    /// <returns>A pending message with a unique ID, or null if the queue is empty</returns>
    Task<TaggedMessage<T>?> DequeueAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a previously dequeued message, removing it from the pending state.
    /// </summary>
    Task AckAsync(
        object ackTag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a previously dequeued message, leaving it in the queue.
    /// </summary>
    Task NackAsync(
        object ackTag,
        bool requeue = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of messages in the queue (excluding pending ones)
    /// </summary>
    Task<int> CountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all messages, including pending ones
    /// </summary>
    Task ClearAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a message in the queue
/// </summary>
public class TaggedMessage<T>
{
    /// <summary>
    /// Actual message payload
    /// </summary>
    public required T Message { get; set; }

    /// <summary>
    /// Acknowledgment tag for the message
    /// </summary>
    public required object AckTag { get; set; }
}
