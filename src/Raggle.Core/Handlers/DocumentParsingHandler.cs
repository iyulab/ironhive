using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Core.Document;
using Raggle.Core.Parsers;
using Raggle.Core.Utils;

namespace Raggle.Core.Handlers;

public class DocumentParsingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentParser[] _parsers;

    public DocumentParsingHandler(
        IDocumentStorage documentStorage,
        IDocumentParser[] parsers)
    {
        _documentStorage = documentStorage;
        _parsers = parsers;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var parser = _parsers.FirstOrDefault(d => d.SupportTypes.Contains(pipeline.Document.ContentType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{pipeline.Document.ContentType}'.");

        var content = await _documentStorage.ReadDocumentFileAsync(
                collectionName: pipeline.Document.CollectionName,
                documentId: pipeline.Document.DocumentId,
                filePath: pipeline.Document.FileName,
                cancellationToken: cancellationToken);
        var sections = await parser.ParseAsync(content, cancellationToken);

        var parsedDocument = new DocumentStructure
        {
            FileName = pipeline.Document.FileName,
            ContentType = pipeline.Document.ContentType,
            ContentLength = content.Length,
            Sections = sections,
        };

        var filename = DocumentFiles.GetParsedFileName(pipeline.Document.FileName);
        var stream = JsonDocumentSerializer.SerializeToStream(parsedDocument);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: filename,
            content: stream,
            cancellationToken: cancellationToken);

        return pipeline;
    }

    public IEnumerable<string> GetSupportedContentTypes()
    {
        var contentTypes = new List<string>();
        foreach (var p in _parsers)
        {
            contentTypes.AddRange(p.SupportTypes);
        }
        return contentTypes;
    }
}
