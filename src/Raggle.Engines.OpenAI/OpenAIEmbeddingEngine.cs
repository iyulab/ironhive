using Raggle.Abstractions.Engines;

namespace Raggle.Engines.OpenAI;

public class OpenAIEmbeddingEngine : IEmbeddingEngine
{
    public Task<IEnumerable<float>> EmbeddingAsync(ICollection<string> inputs)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmbeddingModel>> GetEmbeddingModelsAsync()
    {
        throw new NotImplementedException();
    }
}
