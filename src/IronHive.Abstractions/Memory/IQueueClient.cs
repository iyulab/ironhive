namespace IronHive.Abstractions.Memory;

public interface IQueueClient : IDisposable
{
    Task EnqueueAsync(string message, CancellationToken cancellationToken);

    Task<string> DequeueAsync(CancellationToken cancellationToken);

    //Task<string> PeekAsync(string id, CancellationToken cancellationToken);

    //Task<string[]> ListAsync(int count, CancellationToken cancellationToken);

    Task<int> CountAsync();

    Task ClearAsync();
}
