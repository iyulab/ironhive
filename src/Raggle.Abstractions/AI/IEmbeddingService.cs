namespace Raggle.Abstractions.AI;

public interface IEmbeddingService
{
    Task<EmbeddingModel[]> GetEmbeddingModelsAsync();

    Task<float[]> EmbeddingAsync(string input, EmbeddingOptions options);

    Task<float[][]> EmbeddingsAsync(ICollection<string> inputs, EmbeddingOptions options);
}
