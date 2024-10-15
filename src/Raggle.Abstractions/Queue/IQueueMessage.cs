namespace Raggle.Abstractions.Queue;

public interface IQueueMessage
{
    Guid ID { get; set; }
    string QueueName { get; set; }

    DateTime CreatedAt { get; set; }
}
