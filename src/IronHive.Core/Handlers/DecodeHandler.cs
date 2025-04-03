using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Files;

namespace IronHive.Core.Handlers;

public class DecodeHandler : IPipelineHandler
{
    private readonly IFileStorageManager _manager;

    public DecodeHandler(IFileStorageManager manager)
    {
        _manager = manager;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var source = context.Source;
        if (source is TextMemorySource textSource)
        {
            var text = textSource.Text;
            context.Payload = text;
            return context;
        }
        else if (source is FileMemorySource fileSource)
        {
            var text = await _manager.DecodeFileAsync(
                provider: fileSource.Provider,
                filePath: fileSource.FilePath,
                providerConfig: fileSource.ProviderConfig,
                cancellationToken: cancellationToken);            
            context.Payload = text;
            return context;
        }
        else if (source is WebMemorySource webSource)
        {
            using var client = new HttpClient();
            var text = await client.GetStringAsync(webSource.Url, cancellationToken);
            context.Payload = text;
            return context;
        }
        else
        {
            throw new NotSupportedException($"Unsupported source type: {source?.GetType().Name}");
        }
    }
}
