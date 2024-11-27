namespace Raggle.Abstractions.Memory;

public interface IDocumentDecoder
{
    /// <summary>
    /// Extract text content from the given file.
    /// </summary>
    Task<object> DecodeAsync(
        Stream data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if the specified content type is supported.
    /// </summary>
    /// <param name="contentType">The MIME type of the content.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    bool IsSupportContentType(string contentType);
}
