using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Files;
using HtmlAgilityPack;

namespace IronHive.Core.Handlers;

public class TextExtractionHandler : IMemoryPipelineHandler
{
    private readonly IFileStorageManager _storages;
    private readonly IFileDecoderManager _decoders;

    public TextExtractionHandler(IFileStorageManager storages, IFileDecoderManager decoders)
    {
        _storages = storages;
        _decoders = decoders;
    }

    public async Task<MemoryPipelineContext> ProcessAsync(MemoryPipelineContext context, CancellationToken cancellationToken)
    {
        var source = context.Source;
        if (source is TextMemorySource textSource)
        {
            var text = textSource.Value;

            context.Payload = text;
            return context;
        }
        else if (source is FileMemorySource fileSource)
        {
            using var stream = await _storages.ReadFileAsync(
                storage: fileSource.Storage,
                filePath: fileSource.FilePath,
                cancellationToken: cancellationToken);
            var text = await _decoders.DecodeAsync(
                filePath: fileSource.FilePath,
                data: stream,
                cancellationToken: cancellationToken);

            context.Payload = text;
            return context;
        }
        else if (source is WebMemorySource webSource)
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(webSource.Url, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var text = doc.DocumentNode.InnerText;

            context.Payload = text;
            return context;
        }
        else
        {
            throw new NotSupportedException($"Unsupported source type: {source?.GetType().Name}");
        }
    }
}
