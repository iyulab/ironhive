using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class DecodingFileHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IContentDecoder[] _decoders;

    public DecodingFileHandler(
        IDocumentStorage documentStorage,
        IContentDecoder[] decoders)
    {
        _documentStorage = documentStorage;
        _decoders = decoders;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var mimeType = "";
        var decoder = _decoders.FirstOrDefault(d => d.IsSupportMimeType(mimeType))
            ?? throw new InvalidOperationException($"No decoder found for MIME type '{mimeType}'.");

        var decodedStream = await decoder.DecodeAsync(new MemoryStream());

        return pipeline;
    }
}
