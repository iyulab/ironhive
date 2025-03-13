using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Memory;
using IronHive.Core.Extensions;
using IronHive.Core.Memory.Decoders;

namespace IronHive.Core.Memory.Handlers;

public class DecodingHandler : IPipelineHandler
{
    private readonly IFileStorage _documentStorage;
    private readonly IEnumerable<IDocumentDecoder> _decoders;

    public DecodingHandler(IServiceProvider service)
    {
        _documentStorage = service.GetRequiredService<IFileStorage>();
        _decoders = service.GetServices<IDocumentDecoder>();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.MimeType == null)
            throw new InvalidOperationException("The document MIME type is not specified.");

        // 문서 파서 선택
        var decoder = _decoders.FirstOrDefault(d => d.IsSupportMimeType(pipeline.MimeType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{pipeline.MimeType}'.");

        // 문서 내용 읽기
        var data = await _documentStorage.ReadDocumentFileAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                filePath: pipeline.FileName,
                cancellationToken: cancellationToken);

        // 문서 파싱
        var content = await decoder.DecodeAsync(data, cancellationToken);
        var section = new DocumentFragment
        {
            Index = 0,
            Unit = decoder switch
            {
                TextDecoder => "line",
                PPTDecoder => "slide",
                PDFDecoder => "page",
                WordDecoder => "paragraph",
                _ => "unknown",
            },
            From = 1,
            To = content.Count,
            Content = content
        };

        // 파싱된 문서 저장
        await _documentStorage.UpsertDocumentJsonAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
            suffix: pipeline.CurrentStep ?? "unknown",
            value: section,
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
