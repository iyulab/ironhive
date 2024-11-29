using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Decoders;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Handlers;

public class DecodingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IEnumerable<IDocumentDecoder> _decoders;

    public DecodingHandler(IServiceProvider service)
    {
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _decoders = service.GetServices<IDocumentDecoder>();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        // 문서 파서 선택
        var decoder = _decoders.FirstOrDefault(d => d.IsSupportContentType(pipeline.FileInfo.ContentType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{pipeline.FileInfo.ContentType}'.");

        // 문서 내용 읽기
        var content = await _documentStorage.ReadDocumentFileAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                filePath: pipeline.FileInfo.FileName,
                cancellationToken: cancellationToken);

        // 문서 파싱
        var sections = await decoder.DecodeAsync(content, cancellationToken) as IEnumerable<DocumentSection>
            ?? throw new InvalidOperationException("Invalid document sections.");
        var parsedDocument = new DecodedDocument
        {
            FileName = pipeline.FileInfo.FileName,
            ContentType = pipeline.FileInfo.ContentType,
            ContentLength = content.Length,
            Sections = sections
        };

        // 파싱 결과 저장
        var filename = DocumentFileHelper.GetDecodedFileName(pipeline.FileInfo.FileName);
        var stream = JsonDocumentSerializer.SerializeToStream(parsedDocument);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            filePath: filename,
            content: stream,
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
