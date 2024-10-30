namespace Raggle.Abstractions.AI;

public class EmbeddingModel
{
    public required string ModelID { get; set; }
    public int? MaxTokens { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Owner { get; set; }
}
