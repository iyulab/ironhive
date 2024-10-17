namespace Raggle.Abstractions.Memory;

public interface IContentDecoder
{
    /// <summary>
    /// Returns true if the decoder supports the given MIME type
    /// </summary>
    bool IsSupportMimeType(string mimeType);

    /// <summary>
    /// Extract content from the given file.
    /// </summary>
    Task<StructuredDocument> DecodeAsync(Stream data, CancellationToken cancellationToken = default);
}
