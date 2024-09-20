namespace Raggle.Abstractions.Engines;

public interface IEmbeddingEngine
{
    Task<EmbeddingModel[]> GetEmbeddingModelsAsync();
    Task<float[]> EmbeddingAsync(string input, EmbeddingOptions options);
    Task<float[][]> EmbeddingsAsync(ICollection<string> inputs, EmbeddingOptions options);
}

public class EmbeddingModel
{
    public required string ModelID { get; set; }
    public int? MaxTokens { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Owner { get; set; }
}
