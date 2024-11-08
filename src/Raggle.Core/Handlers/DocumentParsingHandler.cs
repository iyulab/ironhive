using Raggle.Abstractions.Memory;
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
        // 문서 파서 선택
        var parser = _parsers.FirstOrDefault(d => d.SupportContentTypes.Contains(pipeline.Document.ContentType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{pipeline.Document.ContentType}'.");

        // 문서 내용 읽기
        var content = await _documentStorage.ReadDocumentFileAsync(
                collectionName: pipeline.Document.CollectionName,
                documentId: pipeline.Document.DocumentId,
                filePath: pipeline.Document.FileName,
                cancellationToken: cancellationToken);

        // 문서 파싱
        var sections = await parser.ParseAsync(content, cancellationToken);
        var parsedDocument = new ParsedDocument
        {
            FileName = pipeline.Document.FileName,
            ContentType = pipeline.Document.ContentType,
            ContentLength = content.Length,
            Sections = sections,
        };

        // 파싱 결과 저장
        var filename = DocumentFileHelper.GetParsedFileName(pipeline.Document.FileName);
        var stream = JsonDocumentSerializer.SerializeToStream(parsedDocument);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: filename,
            content: stream,
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
