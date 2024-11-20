using Raggle.Core.Memory.Document;

namespace Raggle.Core.Memory.Decoders;

public interface IDocumentDecoder
{
    /// <summary>
    /// Supported MIME types in this decoder.
    /// </summary>
    public IEnumerable<string> SupportContentTypes { get; }

    /// <summary>
    /// Extract text content from the given file.
    /// </summary>
    Task<IEnumerable<DocumentSection>> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default);
}
