using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class DecodingHandler : IPipelineHandler
{
    public IContentDecoder[] Decoders { get; }

    public DecodingHandler(IContentDecoder[] decoders)
    {
        Decoders = decoders;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var mimeType = "";
        var decoder = Decoders.FirstOrDefault(d => d.IsSupportMimeType(mimeType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{mimeType}'.");

        var decodedStream = await decoder.DecodeAsync(new MemoryStream());

        return pipeline;
    }
}
