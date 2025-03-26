using IronHive.Abstractions.Memory;
using System.Collections.Concurrent;

namespace IronHive.Storages.Local;

public class LocalQueueStorage : IQueueStorage
{
    // Thread-safe concurrent queue for storing items
    private readonly ConcurrentQueue<object?> _queue = new ConcurrentQueue<object?>();

    public void Dispose()
    {
        while (!_queue.IsEmpty)
        {
            _queue.TryDequeue(out _);
        }
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task EnqueueAsync<T>(T item, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(item);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T?> DequeueAsync<T>(CancellationToken cancellationToken = default)
    {
        while (_queue.TryDequeue(out object? item))
        {
            if (item is T typedItem)
            {
                return Task.FromResult((T?)typedItem);
            }
        }

        return Task.FromResult(default(T));
    }

    /// <inheritdoc />
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_queue.Count);
    }

    /// <inheritdoc />
    public Task<T?> PeekAsync<T>(CancellationToken cancellationToken = default)
    {
        foreach (var item in _queue)
        {
            if (item is T typedItem)
            {
                return Task.FromResult((T?)typedItem);
            }
        }

        return Task.FromResult(default(T));
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        while (!_queue.IsEmpty)
        {
            _queue.TryDequeue(out _);
        }
        return Task.CompletedTask;
    }
}
