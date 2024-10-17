using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class EmbeddingDataHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IEmbeddingEngine _embeddingEngine;

    public EmbeddingDataHandler(
        IDocumentStorage documentStorage,
        IVectorStorage vectorStorage,
        IEmbeddingEngine embeddingEngine)
    {
        _documentStorage = documentStorage;
        _vectorStorage = vectorStorage;
        _embeddingEngine = embeddingEngine;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Metadata.TryGetValue("Chunks", out var chunksObj) && chunksObj is List<string> chunks)
        {
            var embeddings = new List<float[]>();
            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingEngine.EmbeddingAsync(chunk, new EmbeddingOptions
                {
                    ModelId = "bert-base-uncased",
                });
                embeddings.Add(embedding);
            }
        }

        return pipeline;
    }
}
