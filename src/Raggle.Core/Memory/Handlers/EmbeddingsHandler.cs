using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Handlers;

public class EmbeddingsHandlerOptions
{
    public string ServiceKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}

public class EmbeddingsHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;

    public EmbeddingsHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _vectorStorage = service.GetRequiredService<IVectorStorage>();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentMetadata<EmbeddingsHandlerOptions>()
            ?? throw new InvalidOperationException("No options found for embeddings handler.");
        var points = new List<VectorPoint>();

        var chunkFiles = await GetChunkedDocumentFilesAsync(pipeline, cancellationToken);
        foreach (var chunkFile in chunkFiles)
        {
            var chunk = await GetChunkedDocumentAsync(pipeline, chunkFile, cancellationToken);

            if (chunk.ExtractedQAPairs != null && chunk.ExtractedQAPairs.Any())
            {
                var questions = chunk.ExtractedQAPairs.Select(x => x.Question).ToArray();
                
                var embedder = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(options.ServiceKey);
                var request = new EmbeddingRequest
                { 
                    Model = options.ModelName, 
                    Input = questions 
                };
                var response = await embedder.EmbeddingsAsync(request, cancellationToken);
                for (var i = 0; i < response.Count(); i++)
                {
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = response.ElementAt(i).Embedding,
                        DocumentId = pipeline.DocumentId,
                        ChunkIndex = chunk.Index,
                        QAPairIndex = i,
                        Tags = pipeline.Tags.ToArray()
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
            {
                var embedder = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(options.ServiceKey);
                var response = await embedder.EmbeddingAsync(options.ModelName, chunk.SummarizedText, cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.DocumentId,
                    ChunkIndex = chunk.Index,
                    Tags = pipeline.Tags.ToArray()
                });
            }
            else if (!string.IsNullOrEmpty(chunk.RawText))
            {
                var embedder = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(options.ServiceKey);
                var response = await embedder.EmbeddingAsync(options.ModelName, chunk.RawText, cancellationToken);

                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.DocumentId,
                    ChunkIndex = chunk.Index,
                    Tags = pipeline.Tags.ToArray()
                });
            }
            else
            {
                throw new InvalidOperationException($"No text content found in the document chunk {chunk.Index}.");
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.CollectionName, points, cancellationToken);
        return pipeline;
    }

    #region Private Methods

    private async Task<IEnumerable<string>> GetChunkedDocumentFilesAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            cancellationToken: cancellationToken);
        return filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));
    }

    private async Task<ChunkedDocument> GetChunkedDocumentAsync(DataPipeline pipeline, string chunkFilePath, CancellationToken cancellationToken)
    {
        var chunkStream = await _documentStorage.ReadDocumentFileAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: chunkFilePath,
            cancellationToken: cancellationToken);
        return JsonDocumentSerializer.Deserialize<ChunkedDocument>(chunkStream);
    }

    #endregion
}
