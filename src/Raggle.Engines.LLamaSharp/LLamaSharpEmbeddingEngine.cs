using LLama.Common;
using LLama;
using Raggle.Abstractions.Engines;

namespace Raggle.Engines.LLamaSharp;

public class LLamaSharpEmbeddingEngine : IEmbeddingEngine, IDisposable
{
    private readonly IDictionary<string, LLamaEmbedder> _models;

    public LLamaSharpEmbeddingEngine(LLamaSharpConfig config)
    {
        _models = LoadModels(config);
    }

    public void Dispose()
    {
        foreach (var model in _models)
        {
            model.Value.Dispose();
        }
        _models.Clear();
        GC.SuppressFinalize(this);
    }

    public IDictionary<string, LLamaEmbedder> LoadModels(LLamaSharpConfig config)
    {
        var models = new Dictionary<string, LLamaEmbedder>();
        var modelPaths = config.ModelPaths;
        foreach (var modelPath in modelPaths)
        {
            var parameters = new ModelParams(modelPath)
            {
                Embeddings = true
            };
            var model = LLamaWeights.LoadFromFile(parameters);
            var embedder = new LLamaEmbedder(model, parameters);
            models.Add(modelPath, embedder);
        }
        return models;
    }

    public Task<EmbeddingModel[]> GetEmbeddingModelsAsync()
    {
        var models = new List<EmbeddingModel>();
        foreach (var model in _models)
        {
            models.Add(new EmbeddingModel
            {
                ModelID = model.Key,
                MaxTokens = (int)model.Value.Context.ContextSize,
            });
        }
        return Task.FromResult(models.ToArray());
    }

    public async Task<float[]> EmbeddingAsync(string input, EmbeddingOptions options)
    {
        if (_models.TryGetValue(options.ModelId, out var model))
        {
            var embedding = await model.GetEmbeddings(input);
            return embedding;
        }
        throw new NotImplementedException();
    }

    public async Task<float[][]> EmbeddingsAsync(ICollection<string> inputs, EmbeddingOptions options)
    {
        var embeddings = new List<float[]>();
        if (_models.TryGetValue(options.ModelId, out var model))
        {
            foreach (var input in inputs)
            {
                var embedding = await model.GetEmbeddings(input);
                embeddings.Add(embedding);
            }
            return embeddings.ToArray();
        }
        throw new NotImplementedException();
    }
}
