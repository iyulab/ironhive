namespace Raggle.Abstractions.Memory.Vector;

/// <summary>
/// Document Or Vector Storage에서 사용할 필터링 조건을 나타냅니다.
/// </summary>
public class MemoryFilter
{
    public List<string> DocumentIds { get; init; }
    public List<string> Tags { get; init; }

    public MemoryFilter(
        IEnumerable<string>? documentIds = null,
        IEnumerable<string>? tags = null)
    {
        DocumentIds = documentIds?.ToList() ?? [];
        Tags = tags?.ToList() ?? [];
    }

    /// <summary>
    /// 필터에 Document ID를 추가합니다.
    /// </summary>
    /// <param name="documentId">필터링할 Document ID</param>
    public MemoryFilter AddDocumentId(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("Document ID cannot be null or empty.", nameof(documentId));
        if (DocumentIds.Contains(documentId))
            throw new ArgumentException("Document ID already exists.", nameof(documentId));

        DocumentIds.Add(documentId);
        return this;
    }

    /// <summary>
    /// 필터에 Tag를 추가합니다.
    /// </summary>
    /// <param name="tag">필터링할 Tag</param>
    public MemoryFilter AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));
        if (Tags.Contains(tag))
            throw new ArgumentException("Tag already exists.", nameof(tag));

        Tags.Add(tag);
        return this;
    }
}
