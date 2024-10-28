namespace Raggle.Abstractions.Memory;

public interface IContentDecoder
{
    /// <summary>
    /// Supported MIME types in this decoder.
    /// </summary>
    public string[] SupportTypes { get; }

    /// <summary>
    /// Extract content from the given file.
    /// </summary>
    Task<IEnumerable<DocumentSection>> DecodeAsync(Stream data, CancellationToken cancellationToken = default);
}
