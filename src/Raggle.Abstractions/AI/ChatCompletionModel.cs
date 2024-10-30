namespace Raggle.Abstractions.AI;

public class ChatCompletionModel
{
    public required string ModelId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Owner { get; set; }
}
