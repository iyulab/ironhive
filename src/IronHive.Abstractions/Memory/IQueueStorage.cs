namespace IronHive.Abstractions.Memory;

/// <summary>
/// Represents a storage for queues
/// </summary>
public interface IQueueStorage : IDisposable
{
    /// <summary>
    /// List all queues
    /// </summary>
    /// <returns>Names of all queues</returns>
    Task<IEnumerable<string>> ListQueuesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds out if a queue exists
    /// </summary>
    /// <returns>if the queue exists</returns>
    Task<bool> ExistsQueueAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new queue
    /// </summary>
    Task CreateQueueAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a queue
    /// </summary>
    Task DeleteQueueAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an item to the queue
    /// </summary>
    /// <typeparam name="T">Type of item to enqueue</typeparam>
    /// <param name="name">Name of the queue</param>
    /// <param name="item">Item to add to the queue</param>
    Task EnqueueAsync<T>(
        string name, 
        T item,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes and returns the first item from the queue of specified type
    /// </summary>
    /// <typeparam name="T">Type of item to dequeue</typeparam>
    /// <param name="name">Name of the queue</param>
    /// <returns>The dequeued item</returns>
    Task<T?> DequeueAsync<T>(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current number of items in the queue
    /// </summary>
    /// <param name="name">Name of the queue</param>
    /// <returns>Number of items in the queue</returns>
    Task<int> CountAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all items from the queue
    /// </summary>
    /// <param name="name">Name of the queue</param>
    Task ClearAsync(
        string name, 
        CancellationToken cancellationToken = default);
}
