namespace Raggle.Abstractions.Memory;

public interface IQueueMessage
{
    string Id { get; set; }

    string? Name { get; set; }

    DateTime CreatedAt { get; set; }
}
