using DocumentFormat.OpenXml.InkML;
using HtmlAgilityPack;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// TextExtractionHandler는 주어진 메모리 소스에서 텍스트를 추출하는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class TextExtractionPipeline : IPipeline<PipelineContext, PipelineContext<string>>
{
    private readonly IFileStorageService _storages;
    private readonly IFileExtractionService<string> _extractor;

    public TextExtractionPipeline(IFileStorageService storages, IFileExtractionService<string> extractor)
    {
        _storages = storages;
        _extractor = extractor;
    }

    /// <inheritdoc />
    public async Task<PipelineContext<string>> InvokeAsync(
        PipelineContext input, 
        CancellationToken cancellationToken = default)
    {
        string text;
        if (input.Source is TextMemorySource textSource)
        {
            text = textSource.Value;
        }
        else if (input.Source is FileMemorySource fileSource)
        {
            using var stream = await _storages.ReadFileAsync(
                storageName: fileSource.StorageName,
                filePath: fileSource.FilePath,
                cancellationToken: cancellationToken);
            text = await _extractor.DecodeAsync(
                fileName: fileSource.FilePath,
                data: stream,
                cancellationToken: cancellationToken);
        }
        else if (input.Source is WebMemorySource webSource)
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(webSource.Url, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            text = doc.DocumentNode.InnerText;
        }
        else
        {
            throw new NotSupportedException($"Unsupported source type: {input.Source.GetType().Name}");
        }

        return new PipelineContext<string>
        {
            Source = input.Source,
            Target = input.Target,
            Payload = text
        };
    }
}
