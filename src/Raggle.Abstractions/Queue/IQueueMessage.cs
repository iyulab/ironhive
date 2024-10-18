namespace Raggle.Abstractions.Queue;

public interface IQueueMessage
{
    string QueueId { get; set; }

    string? QueueName { get; set; }

    DateTime CreatedAt { get; set; }
}
