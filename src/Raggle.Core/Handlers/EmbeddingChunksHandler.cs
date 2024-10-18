using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class EmbeddingChunksHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IEmbeddingEngine _embeddingEngine;

    public EmbeddingChunksHandler(
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
        var chunkPaths = await GetChunkFilesAsync(pipeline, cancellationToken);

        var embeddingModel = "???";
        var embeddings = new List<VectorRecord>();
        foreach (var filepath in chunkPaths)
        {
            var chunk = await ReadChunkFileAsync(pipeline, filepath, cancellationToken);
            var embedding = await _embeddingEngine.EmbeddingAsync(chunk, new EmbeddingOptions
            {
                ModelId = embeddingModel,
            });
            embeddings.Add(new VectorRecord
            {
                DocumentId = pipeline.DocumentId,
                EmbeddingModel = embeddingModel,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow,
                Tags = pipeline.Tags ?? [],
                LastUpdatedAt = DateTime.UtcNow,
            });
        }

        await _vectorStorage.UpsertRecordsAsync(
            collection: pipeline.CollectionName,
            records: embeddings,
            cancellationToken: cancellationToken);

        return pipeline;
    }

    private async Task<IEnumerable<string>> GetChunkFilesAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var files = await _documentStorage.GetDocumentFilesAsync(
            collection: pipeline.CollectionName, 
            documentId: pipeline.DocumentId, 
            cancellationToken: cancellationToken);

        var chunkfiles = files.Where(f => f.StartsWith("chunks/"));
        return chunkfiles;
    }

    private async Task<string> ReadChunkFileAsync(DataPipeline pipeline, string filepath, CancellationToken cancellationToken)
    {
        var stream = await _documentStorage.ReadDocumentFileAsync(
            collection: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: filepath,
            cancellationToken: cancellationToken);

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
