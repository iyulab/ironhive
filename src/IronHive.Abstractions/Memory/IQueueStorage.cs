namespace IronHive.Abstractions.Memory;

/// <summary>
/// Represents a storage for queues
/// </summary>
public interface IQueueStorage : IDisposable
{
    /// <summary>
    /// Adds an message to the queue
    /// </summary>
    /// <param name="message">Message to enqueue</param>
    Task EnqueueAsync(
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes and returns the first message from the queue
    /// </summary>
    /// <returns>The dequeued message, or null if the queue is empty</returns>
    Task<string?> DequeueAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current number of messages in the queue
    /// </summary>
    /// <returns>Number of messages in the queue</returns>
    Task<int> CountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all messages from the queue
    /// </summary>
    Task ClearAsync(
        CancellationToken cancellationToken = default);
}

//public class MessageEnqueuedEventArgs : EventArgs
//{
//    public string QueueName { get; }
//    public string Message { get; }

//    public MessageEnqueuedEventArgs(string queueName, string message)
//    {
//        QueueName = queueName;
//        Message = message;
//    }
//}
