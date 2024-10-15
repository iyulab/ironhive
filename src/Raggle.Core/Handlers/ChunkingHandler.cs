using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class ChunkingHandler : IPipelineHandler
{
    private const int ChunkSize = 500;

    public Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Metadata.TryGetValue("DecodedContent", out var decodedContentObj) && decodedContentObj is string decodedContent)
        {
            var chunks = new List<string>();
            for (int i = 0; i < decodedContent.Length; i += ChunkSize)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                chunks.Add(decodedContent.Substring(i, Math.Min(ChunkSize, decodedContent.Length - i)));
            }

            pipeline.Metadata["Chunks"] = chunks;
        }

        return Task.FromResult(pipeline);
    }
}
