using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class SaveMemoryHandler
{
    private readonly IVectorStorage _storage;

    public SaveMemoryHandler(IVectorStorage vectorStorage)
    {
        _storage = vectorStorage;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Metadata.TryGetValue("Chunks", out var chunksObj) && chunksObj is List<string> chunks &&
            pipeline.Metadata.TryGetValue("Embeddings", out var embeddingsObj) && embeddingsObj is List<float[]> embeddings)
        {
            var records = new List<VectorRecord>();

            for (int i = 0; i < chunks.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;


            }
        }

        return pipeline;
    }
}
