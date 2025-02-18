namespace Raggle.Abstractions.Memory;

public interface IQueueClient : IDisposable
{
    Task EnqueueAsync(IQueueMessage message, CancellationToken cancellationToken);

    Task<IQueueMessage> DequeueAsync(CancellationToken cancellationToken);

    //Task<IQueueMessage> PeekAsync(string id, CancellationToken cancellationToken);

    //Task<IQueueMessage[]> ListAsync(int count, CancellationToken cancellationToken);

    Task<int> CountAsync();

    Task ClearAsync();
}
