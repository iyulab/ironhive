using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class EmbeddingHandler : IPipelineHandler
{
    public IEmbeddingEngine EmbeddingEngine { get; }

    public EmbeddingHandler(IEmbeddingEngine embeddingEngine)
    {
        EmbeddingEngine = embeddingEngine;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Metadata.TryGetValue("Chunks", out var chunksObj) && chunksObj is List<string> chunks)
        {
            var embeddings = new List<float[]>();
            foreach (var chunk in chunks)
            {
                var embedding = await EmbeddingEngine.EmbeddingAsync(chunk, new EmbeddingOptions
                {
                    ModelId = "bert-base-uncased",
                });
                embeddings.Add(embedding);
            }
        }

        return pipeline;
    }
}
