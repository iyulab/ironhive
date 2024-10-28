using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class DecodeDocumentHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IContentDecoder[] _decoders;

    public DecodeDocumentHandler(
        IDocumentStorage documentStorage,
        IContentDecoder[] decoders)
    {
        _documentStorage = documentStorage;
        _decoders = decoders;
    }

    public IEnumerable<string> GetSupportedContentTypes()
    {
        var contentTypes = new List<string>();
        foreach (var decoder in _decoders)
        {
            contentTypes.AddRange(decoder.SupportTypes);
        }
        return contentTypes;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        //if (pipeline.UploadFile != null)
        //{
        //    await ParseDocumentAndSaveAsync(
        //        collection: pipeline.CollectionName,
        //        documentId: pipeline.DocumentId,
        //        file: pipeline.UploadFile,
        //        cancellationToken: cancellationToken);
        //}
        //else
        //{
        //    var filter = new MemoryFilterBuilder().AddDocumentId(pipeline.DocumentId).Build();
        //    var records = await _documentStorage.FindDocumentRecordsAsync(
        //        collection: pipeline.CollectionName,
        //        filter: filter,
        //        cancellationToken: cancellationToken);
        //    var record = records.FirstOrDefault()
        //        ?? throw new InvalidOperationException($"Document '{pipeline.DocumentId}' not found.");

        //    var content = await _documentStorage.ReadDocumentFileAsync(
        //        collection: pipeline.CollectionName,
        //        documentId: pipeline.DocumentId,
        //        filePath: record.FileName,
        //        cancellationToken: cancellationToken);

        //    var file = new UploadFile
        //    {
        //        FileName = record.FileName,
        //        ContentType = record.ContentType,
        //        Content = content,
        //    };

        //    await ParseDocumentAndSaveAsync(
        //        collection: pipeline.CollectionName,
        //        documentId: pipeline.DocumentId,
        //        file: file,
        //        cancellationToken: cancellationToken);
        //}
        return pipeline;
    }

    //private async Task ParseDocumentAndSaveAsync(
    //    string collection,
    //    string documentId,
    //    UploadFile file,
    //    CancellationToken cancellationToken)
    //{
    //    var decoder = _decoders.FirstOrDefault(d => d.SupportTypes.Contains(file.ContentType))
    //        ?? throw new InvalidOperationException($"No decoder found for MIME type '{file.ContentType}'.");

    //    var contents = await decoder.DecodeAsync(file.Content, cancellationToken);
    //    var jsonString = JsonSerializer.Serialize(new StructuredDocument
    //    {
    //        DocumentId = documentId,
    //        FileName = file.FileName,
    //        ContentType = file.ContentType,
    //        Size = file.Content.Length,
    //        Contents = contents,
    //    });
    //    var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

    //    await _documentStorage.WriteDocumentFileAsync(
    //        collection: collection,
    //        documentId: documentId,
    //        filePath: $"{file.FileName}.structured.json",
    //        content: uploadStream,
    //        cancellationToken: cancellationToken);
    //}
}
