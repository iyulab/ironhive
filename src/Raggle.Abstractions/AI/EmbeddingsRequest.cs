namespace Raggle.Abstractions.AI;

public class EmbeddingsRequest
{
    /// <summary>
    /// The Embedding Model Name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The input text to embed.
    /// </summary>
    public string[] Input { get; set; } = [];
}
