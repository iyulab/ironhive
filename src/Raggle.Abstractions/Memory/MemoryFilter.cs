namespace Raggle.Abstractions.Memory;

/// <summary>
/// Document Or Vector Storage에서 사용할 필터링 조건을 나타냅니다.
/// </summary>
public class MemoryFilter
{
    public IReadOnlyCollection<string>? DocumentIds { get; }
    public IReadOnlyCollection<string>? Tags { get; }

    public MemoryFilter(
        IReadOnlyCollection<string>? documentIds,
        IReadOnlyCollection<string>? tags)
    {
        DocumentIds = documentIds;
        Tags = tags;
    }
}

/// <summary>
/// MemoryFilter 인스턴스를 생성하는 빌더 클래스입니다.
/// </summary>
public class MemoryFilterBuilder
{
    private readonly List<string> _documentIds = new();
    private readonly List<string> _tags = new();

    /// <summary>
    /// 필터에 Document ID를 추가합니다.
    /// </summary>
    /// <param name="documentId">필터링할 Document ID</param>
    public MemoryFilterBuilder AddDocumentId(string documentId)
    {
        if (!string.IsNullOrWhiteSpace(documentId))
        {
            _documentIds.Add(documentId);
        }
        return this;
    }

    /// <summary>
    /// 필터에 Tag를 추가합니다.
    /// </summary>
    /// <param name="tag">필터링할 Tag</param>
    public MemoryFilterBuilder AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            _tags.Add(tag);
        }
        return this;
    }

    /// <summary>
    /// 지정된 기준으로 MemoryFilter 인스턴스를 생성합니다.
    /// </summary>
    public MemoryFilter Build()
    {
        return new MemoryFilter(
            _documentIds.Any() ? _documentIds.AsReadOnly() : null,
            _tags.Any() ? _tags.AsReadOnly() : null
        );
    }
}
