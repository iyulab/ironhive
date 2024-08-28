using LLama.Common;
using LLama;

namespace Raggle.Extensions.LLamaSharp;

public class LLamaSharpEmbeddingEngine
{
    private readonly ModelParams _params;
    public LLamaWeights? Model { get; private set; }

    public LLamaSharpEmbeddingEngine(string modelPath)
    {
        _params = GetParameters(modelPath);
        LoadModel();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void LoadModel()
    {
        Model = LLamaWeights.LoadFromFile(_params);
    }

    public async Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
    {
        if (Model == null)
        {
            throw new InvalidOperationException("Model is not loaded.");
        }

        var embedder = new LLamaEmbedder(Model, _params);
        var embeddings = await embedder.GetEmbeddings(text, cancellationToken);
        return embeddings;
    }

    private static ModelParams GetParameters(string modelPath)
    {
        return new ModelParams(modelPath)
        {
            Embeddings = true
        };
    }
}
