namespace Raggle.Abstractions.Engines;

public interface IEmbeddingEngine
{
    Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync();
    Task<IEnumerable<float>> EmbeddingAsync(ICollection<string> inputs);
}

public class EmbeddingModel
{
    public required string ModelID { get; set; }
}