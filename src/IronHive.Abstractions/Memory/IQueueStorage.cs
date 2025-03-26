namespace IronHive.Abstractions.Memory;

public interface IQueueStorage : IDisposable
{
    /// <summary>
    /// Adds an item to the queue
    /// </summary>
    /// <typeparam name="T">Type of item to enqueue</typeparam>
    /// <param name="item">Item to add to the queue</param>
    Task EnqueueAsync<T>(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes and returns the first item from the queue of specified type
    /// </summary>
    /// <typeparam name="T">Type of item to dequeue</typeparam>
    /// <returns>The dequeued item</returns>
    Task<T?> DequeueAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current number of items in the queue
    /// </summary>
    /// <returns>Number of items in the queue</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first item of specified type without removing it
    /// </summary>
    /// <typeparam name="T">Type of item to peek</typeparam>
    /// <returns>The first item of specified type</returns>
    Task<T?> PeekAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all items from the queue
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
