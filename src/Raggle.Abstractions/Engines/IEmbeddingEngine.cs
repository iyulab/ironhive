namespace Raggle.Abstractions.Engines;

public interface IEmbeddingEngine
{
    Task<EmbeddingModel[]> GetEmbeddingModelsAsync();

    Task<float[]> EmbeddingAsync(string input, EmbeddingOptions options);

    Task<float[][]> EmbeddingsAsync(ICollection<string> inputs, EmbeddingOptions options);
}
