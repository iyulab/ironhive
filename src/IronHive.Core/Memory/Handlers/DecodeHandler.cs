using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Files;

namespace IronHive.Core.Memory.Handlers;

public class DecodeHandler : IPipelineHandler
{
    private readonly IFileManager _manager;

    public DecodeHandler(IFileManager manager)
    {
        _manager = manager;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var source = pipeline.Source;
        if (source is TextMemorySource textSource)
        {
            var text = textSource.Text;
            pipeline.Payload = text;
            return pipeline;
        }
        else if (source is FileMemorySource fileSource)
        {
            var data = fileSource.Data;
            var text = await _manager.DecodeAsync(
                fileName: Path.GetFileName(fileSource.FilPath),
                data: data, 
                cancellationToken: cancellationToken);
            pipeline.Payload = text;
            return pipeline;
        }
        else
        {
            throw new InvalidOperationException("Unsupported source type.");
        }
    }
}
