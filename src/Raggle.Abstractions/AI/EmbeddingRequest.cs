namespace Raggle.Abstractions.AI;

public class EmbeddingRequest
{
    /// <summary>
    /// The Embedding Model Name.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// The input text to embed.
    /// </summary>
    public string[] Input { get; set; } = [];
}
