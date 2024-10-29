using DocumentFormat.OpenXml.Drawing.Charts;
using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Memory.Vector;
using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Handlers;

public class DocumentEmbeddingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;
    private readonly IEmbeddingEngine _embeddingEngine;

    public DocumentEmbeddingHandler(
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
        var modelOption = new EmbeddingOptions
        {
            ModelId = ""
        };
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        var points = new List<VectorPoint>();
        foreach (var chunk in chunks)
        {
            if (chunk.QAItems != null && chunk.QAItems.Any())
            {
                var questions = chunk.QAItems.Select(x => x.Question).ToList();
                var embeddings = await _embeddingEngine.EmbeddingsAsync(questions, modelOption);
                var answers = chunk.QAItems.Select(x => x.Answer).ToList();
                for (int i = 0; i < questions.Count; i++)
                {
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        DocumentId = pipeline.Document.DocumentId,
                        Tags = pipeline.Document.Tags,
                        Text = answers[i],
                        Vectors = embeddings[i],
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(chunk.Text))
            {
                var embedding = await _embeddingEngine.EmbeddingAsync(chunk.Text, modelOption);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    DocumentId = pipeline.Document.DocumentId,
                    Tags = pipeline.Document.Tags,
                    Text = chunk.Text,
                    Vectors = embedding,
                });
            }
            else
            {
                continue;
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
        var chunkFilePaths = filePaths.Where(x => x.EndsWith(DocumentFiles.ChunkedFileExtension));

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
