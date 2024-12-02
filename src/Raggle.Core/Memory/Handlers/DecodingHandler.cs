using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;

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
        if (pipeline.Source.MimeType == null)
            throw new InvalidOperationException("The document MIME type is not specified.");

        // 문서 파서 선택
        var decoder = _decoders.FirstOrDefault(d => d.IsSupportMimeType(pipeline.Source.MimeType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{pipeline.Source.MimeType}'.");

        // 문서 내용 읽기
        var data = await _documentStorage.ReadDocumentFileAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                filePath: pipeline.Source.FileName,
                cancellationToken: cancellationToken);

        // 문서 파싱
        var extractedFIle = await decoder.DecodeAsync(pipeline, data, cancellationToken);

        // 파싱된 문서 저장
        var filename = pipeline.GetCurrentStepFileName();
        await _documentStorage.WriteJsonDocumentFileAsync(
            pipeline.CollectionName,
            pipeline.DocumentId,
            filename,
            extractedFIle,
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
