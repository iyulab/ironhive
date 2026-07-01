using HtmlAgilityPack;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Workflow;

namespace IronHive.Core.Memory.Pipelines;

public class TextExtractionPipeline : IMemoryPipeline
{
    private readonly IFileStorageService _storages;
    private readonly IFileParserService _parser;

    public TextExtractionPipeline(IFileStorageService storages, IFileParserService parser)
    {
        _storages = storages;
        _parser = parser;
    }

    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context,
        CancellationToken cancellationToken = default)
    {
        string text;
        if (context.Source is TextMemorySource textSource)
        {
            text = textSource.Value;
        }
        else if (context.Source is FileMemorySource fileSource)
        {
            using var stream = await _storages.ReadFileAsync(
                storageName: fileSource.StorageName,
                filePath: fileSource.FilePath,
                cancellationToken: cancellationToken);

            var blocks = await _parser.ParseAsync(
                fileName: fileSource.FilePath,
                data: stream,
                cancellationToken: cancellationToken);

            text = string.Join(Environment.NewLine, blocks
                .OfType<TextBlock>()
                .Select(b => b.Text));
        }
        else if (context.Source is WebMemorySource webSource)
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(webSource.Url, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            text = doc.DocumentNode.InnerText;
        }
        else
        {
            throw new NotSupportedException($"Unsupported source type: {context.Source.GetType().Name}");
        }

        context.Payload.Add("text", text);
        return TaskStepResult.Success();
    }
}
