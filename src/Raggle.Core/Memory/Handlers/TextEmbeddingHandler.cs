using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Handlers;

public class TextEmbeddingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IEmbeddingService _embeddingService;
    private readonly string _embeddingModel;

    public TextEmbeddingHandler(
        IDocumentStorage documentStorage,
        IVectorStorage vectorStorage,
        IEmbeddingService embeddingService,
        string embeddingModel)
    {
        _documentStorage = documentStorage;
        _vectorStorage = vectorStorage;
        _embeddingService = embeddingService;
        _embeddingModel = embeddingModel;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var points = new List<VectorPoint>();

        var chunkFiles = await GetChunkedDocumentFilesAsync(pipeline, cancellationToken);
        foreach (var chunkFile in chunkFiles)
        {
            var chunk = await GetChunkedDocumentAsync(pipeline, chunkFile, cancellationToken);

            if (chunk.ExtractedQAPairs != null && chunk.ExtractedQAPairs.Any())
            {
                var questions = chunk.ExtractedQAPairs.Select(x => x.Question).ToArray();
                var response = await _embeddingService.EmbeddingsAsync(new EmbeddingRequest
                {
                    Model = _embeddingModel,
                    Input = questions
                }, cancellationToken);
                for (var i = 0; i < response.Count(); i++)
                {
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = response.ElementAt(i).Embedding,
                        DocumentId = pipeline.Document.DocumentId,
                        ChunkIndex = chunk.Index,
                        QAPairIndex = i,
                        Tags = pipeline.Document.Tags
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
            {
                var response = await _embeddingService.EmbeddingAsync(_embeddingModel, chunk.SummarizedText, cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.Document.DocumentId,
                    ChunkIndex = chunk.Index,
                    Tags = pipeline.Document.Tags
                });
            }
            else if (!string.IsNullOrEmpty(chunk.RawText))
            {
                var response = await _embeddingService.EmbeddingAsync(
                    _embeddingModel, 
                    chunk.RawText, 
                    cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
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

    private async Task<IEnumerable<string>> GetChunkedDocumentFilesAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            cancellationToken: cancellationToken);
        return filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));
    }

    private async Task<ChunkedDocument> GetChunkedDocumentAsync(DataPipeline pipeline, string chunkFilePath, CancellationToken cancellationToken)
    {
        var chunkStream = await _documentStorage.ReadDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: chunkFilePath,
            cancellationToken: cancellationToken);
        return JsonDocumentSerializer.Deserialize<ChunkedDocument>(chunkStream);
    }

    #endregion
}
