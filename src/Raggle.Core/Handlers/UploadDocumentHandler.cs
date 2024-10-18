using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class UploadDocumentHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;

    public UploadDocumentHandler(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        if (pipeline.UploadFile is null)
            throw new ArgumentNullException($"{nameof(pipeline.UploadFile)} is required to upload a document.");

        var document = new DocumentRecord
        {
            EmbeddingStatus = EmbeddingStatus.Pending,
            DocumentId = pipeline.DocumentId,
            FileName = pipeline.UploadFile.FileName,
            ContentType = pipeline.UploadFile.ContentType,
            Size = pipeline.UploadFile.Content.Length,
            Tags = pipeline.Tags ?? [],
            CreatedAt = DateTime.UtcNow,
        };

        await _documentStorage.UpsertDocumentRecordAsync(
            collection: pipeline.CollectionName,
            document: document,
            cancellationToken: cancellationToken);

        try
        {
            await _documentStorage.WriteDocumentFileAsync(
            collection: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: pipeline.UploadFile.FileName,
            content: pipeline.UploadFile.Content,
            cancellationToken: cancellationToken);
        }
        catch
        {
            await _documentStorage.DeleteDocumentRecordAsync(
                collection: pipeline.CollectionName,
                documentId: pipeline.DocumentId);
            throw;
        }

        return pipeline;
    }
}
