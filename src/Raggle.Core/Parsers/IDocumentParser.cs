using Raggle.Core.Document;

namespace Raggle.Core.Parsers;

public interface IDocumentParser
{
    /// <summary>
    /// Supported MIME types in this decoder.
    /// </summary>
    public string[] SupportContentTypes { get; }

    /// <summary>
    /// Extract content from the given file.
    /// </summary>
    Task<IEnumerable<DocumentSection>> ParseAsync(Stream data, CancellationToken cancellationToken = default);
}
