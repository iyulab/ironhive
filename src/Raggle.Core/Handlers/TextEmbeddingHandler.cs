using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Memory.Vector;
using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Handlers;

public class TextEmbeddingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IEmbeddingService _embeddingService;
    private readonly EmbeddingOptions _embeddingOptions;

    public TextEmbeddingHandler(
        IDocumentStorage documentStorage,
        IVectorStorage vectorStorage,
        IEmbeddingService embeddingService,
        EmbeddingOptions embeddingOptions)
    {
        _documentStorage = documentStorage;
        _vectorStorage = vectorStorage;
        _embeddingService = embeddingService;
        _embeddingOptions = embeddingOptions;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        var points = new List<VectorPoint>();
        foreach (var chunk in chunks)
        {
            float[][] embeddings;
            if (chunk.ExtractedQuestions != null && chunk.ExtractedQuestions.Any())
            {
                embeddings = await _embeddingService.EmbeddingsAsync(
                    chunk.ExtractedQuestions.ToArray(),
                    _embeddingOptions);
            }
            else if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
            {
                embeddings = await _embeddingService.EmbeddingsAsync(
                    [chunk.SummarizedText],
                    _embeddingOptions);
            }
            else if (!string.IsNullOrEmpty(chunk.RawText))
            {
                embeddings = await _embeddingService.EmbeddingsAsync(
                    [chunk.RawText],
                    _embeddingOptions);
            }
            else
            {
                continue;
            }

            if (embeddings.Length == 0)
                continue;

            for (var i = 0; i < embeddings.Length; i++)
            {
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = embeddings[i],
                    DocumentId = pipeline.Document.DocumentId,
                    ChunkIndex = chunk.ChunkIndex,
                    Tags = pipeline.Document.Tags
                });
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.Document.CollectionName, points, cancellationToken);
        return pipeline;
    }

    private async Task<IEnumerable<DocumentChunk>> GetDocumentChunksAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            pipeline.Document.CollectionName, 
            pipeline.Document.DocumentId, 
            cancellationToken);
        var chunkFilePaths = filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));

        var chunks = new List<DocumentChunk>();
        foreach (var chunkFilePath in chunkFilePaths)
        {
            var chunkStream = await _documentStorage.ReadDocumentFileAsync(
                pipeline.Document.CollectionName,
                pipeline.Document.DocumentId,
                chunkFilePath,
                cancellationToken);

            var chunk = JsonDocumentSerializer.Deserialize<DocumentChunk>(chunkStream);
            chunks.Add(chunk);
        }
        return chunks;
    }
}
