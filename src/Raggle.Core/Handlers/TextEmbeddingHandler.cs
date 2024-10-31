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
        var points = new List<VectorPoint>();

        var chunkFiles = await GetDocumentChunkFilesAsync(pipeline, cancellationToken);
        foreach (var chunkFile in chunkFiles)
        {
            var chunk = await GetDocumentChunkAsync(pipeline, chunkFile, cancellationToken);

            if (chunk.ExtractedQAPairs != null && chunk.ExtractedQAPairs.Any())
            {
                var questions = chunk.ExtractedQAPairs.Select(x => x.Question).ToList();
                var embeddings = await _embeddingService.EmbeddingsAsync(questions, _embeddingOptions);
                for (var i = 0; i < embeddings.Length; i++)
                {
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = embeddings[i],
                        DocumentId = pipeline.Document.DocumentId,
                        ChunkIndex = chunk.Index,
                        QAPairIndex = i,
                        Tags = pipeline.Document.Tags
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
            {
                var embedding = await _embeddingService.EmbeddingAsync(chunk.SummarizedText, _embeddingOptions);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = embedding,
                    DocumentId = pipeline.Document.DocumentId,
                    ChunkIndex = chunk.Index,
                    Tags = pipeline.Document.Tags
                });
            }
            else if (!string.IsNullOrEmpty(chunk.RawText))
            {
                var embedding = await _embeddingService.EmbeddingAsync(chunk.RawText, _embeddingOptions);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = embedding,
                    DocumentId = pipeline.Document.DocumentId,
                    ChunkIndex = chunk.Index,
                    Tags = pipeline.Document.Tags
                });
            }
            else
            {
                throw new InvalidOperationException($"No text content found in the document chunk {chunk.Index}.");
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.Document.CollectionName, points, cancellationToken);
        return pipeline;
    }

    #region Private Methods

    private async Task<IEnumerable<string>> GetDocumentChunkFilesAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            cancellationToken: cancellationToken);
        return filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));
    }

    private async Task<DocumentChunk> GetDocumentChunkAsync(DataPipeline pipeline, string chunkFilePath, CancellationToken cancellationToken)
    {
        var chunkStream = await _documentStorage.ReadDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: chunkFilePath,
            cancellationToken: cancellationToken);
        return JsonDocumentSerializer.Deserialize<DocumentChunk>(chunkStream);
    }

    #endregion
}
