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
    Task<DocumentContent> DecodeAsync(Stream data, CancellationToken cancellationToken = default);
}

public class DocumentContent
{
    public Guid DocumentID { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }

    public ContentBlock[] Blocks { get; set; }
}

public enum ContentBlockType
{

}

public class ContentBlock
{
    public ContentBlockType Type { get; set; }
    public string Content { get; set; }
}
