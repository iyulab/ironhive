namespace Raggle.Abstractions.Engines;

public class ChatModel
{
    public required string ModelId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Owner { get; set; }
}
