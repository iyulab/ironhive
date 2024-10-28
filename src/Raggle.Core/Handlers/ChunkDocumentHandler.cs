using Raggle.Abstractions.Memory;
using System.Text;

namespace Raggle.Core.Handlers;

public class ChunkDocumentHandler : IPipelineHandler
{
    private const int ChunkSize = 500;
    private readonly IDocumentStorage _documentStorage;

    public ChunkDocumentHandler(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        //var structured = await GetStructuredDocumentAsync(pipeline, cancellationToken);

        //int index = 0;
        //foreach (var content in structured.Contents ?? [])
        //{
        //    if (content is TextDocumentContent textContent)
        //    {
        //        await SaveChunkFileAsync(pipeline, textContent.Text, index, cancellationToken);
        //    }
        //    else if (content is ImageDocumentContent imageContent)
        //    {
        //        throw new InvalidOperationException("Unsupported content type.");
        //    }
        //    else if (content is TableDocumentContent tableContent)
        //    {
        //        throw new InvalidOperationException("Unsupported content type.");
        //    }
        //    index++;
        //}
        return pipeline;
    }
    
    //private async Task<StructuredDocument> GetStructuredDocumentAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    //{
    //    var files = await _documentStorage.GetDocumentFilesAsync(
    //        collectionName: pipeline.CollectionName,
    //        documentId: pipeline.DocumentId);

    //    var filepath = files.Where(f => f.EndsWith("structured.json")).FirstOrDefault()
    //        ?? throw new InvalidOperationException("Structured document not found.");

    //    var stream = await _documentStorage.ReadDocumentFileAsync(
    //        collectionName: pipeline.CollectionName,
    //        documentId: pipeline.DocumentId,
    //        filePath: filepath,
    //        cancellationToken: cancellationToken);

    //    var structuctured = JsonSerializer.Deserialize<StructuredDocument>(stream)
    //        ?? throw new InvalidOperationException("Structured document is invalid.");
    //    return structuctured;
    //}

    private async Task SaveChunkFileAsync(DataPipeline pipeline, string content, int index, CancellationToken cancellationToken)
    {
        var chunkId = $"{pipeline.DocumentId}_{index:D4}";
        var chunkPath = Path.Combine("chunks", chunkId);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: chunkPath,
            content: new MemoryStream(Encoding.UTF8.GetBytes(content)),
            cancellationToken: cancellationToken);
    }
}
