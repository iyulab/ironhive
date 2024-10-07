using Raggle.Abstractions.Memories;
using System.Text;

namespace Raggle.Queue.LocalDisk;

public class DiskMemoryQueue : IMemoryQueue
{
    private readonly string _queueDirectory;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private bool _disposed = false;

    public DiskMemoryQueue(string queueDirectory)
    {
        if (string.IsNullOrWhiteSpace(queueDirectory))
            throw new ArgumentException("Queue directory path cannot be null or whitespace.", nameof(queueDirectory));

        _queueDirectory = queueDirectory;
        Directory.CreateDirectory(_queueDirectory);
    }

    /// <summary>
    /// Enqueues a message to the disk-based queue.
    /// </summary>
    /// <param name="message">The message to enqueue.</param>
    /// <returns>A task that represents the asynchronous enqueue operation.</returns>
    public async Task EnqueueAsync(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}.msg";
        string filePath = Path.Combine(_queueDirectory, fileName);

        byte[] data = Encoding.UTF8.GetBytes(message);

        // Ensure exclusive access during file write
        await _semaphore.WaitAsync();
        try
        {
            await File.WriteAllBytesAsync(filePath, data);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Dequeues a message from the disk-based queue.
    /// </summary>
    /// <returns>A task that represents the asynchronous dequeue operation. The task result contains the dequeued message, or null if the queue is empty.</returns>
    public async Task<string?> DequeueAsync()
    {
        // Ensure exclusive access during dequeue operation
        await _semaphore.WaitAsync();
        try
        {
            var files = Directory.GetFiles(_queueDirectory, "*.msg")
                                 .OrderBy(f => f)
                                 .ToList();

            if (!files.Any())
                return null;

            string oldestFile = files.First();
            byte[] data = await File.ReadAllBytesAsync(oldestFile);
            string message = Encoding.UTF8.GetString(data);

            File.Delete(oldestFile);

            return message;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets the approximate number of messages in the queue.
    /// </summary>
    /// <returns>The count of message files in the queue directory.</returns>
    public int Count()
    {
        return Directory.GetFiles(_queueDirectory, "*.msg").Length;
    }

    /// <summary>
    /// Clears all messages from the queue.
    /// </summary>
    public void Clear()
    {
        _semaphore.Wait();
        try
        {
            var files = Directory.GetFiles(_queueDirectory, "*.msg");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Disposes the semaphore.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
