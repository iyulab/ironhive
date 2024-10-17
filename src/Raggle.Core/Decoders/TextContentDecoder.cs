using Raggle.Abstractions.Memory;

namespace Raggle.Core.Extractors;

public class TextContentDecoder : IContentDecoder
{
    public Task<StructuredDocument> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool IsSupportMimeType(string mimeType)
    {
        throw new NotImplementedException();
    }
}
