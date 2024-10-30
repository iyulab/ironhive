namespace Raggle.Core.Document;

/// <summary>
/// 문서의 전체 정보를 나타내는 클래스입니다.
/// </summary>
public class ParsedDocument
{
    /// <summary>
    /// 문서의 파일 이름을 저장합니다.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 문서의 콘텐츠 유형(MIME 타입)을 저장합니다.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 문서의 콘텐츠 길이(바이트 단위)를 저장합니다.
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// 문서를 분리한 섹션들의 컬렉션을 저장합니다.
    /// 페이지, 슬라이드, 섹션 등으로 나뉠 수 있습니다.
    /// </summary>
    public IEnumerable<DocumentSection> Sections { get; set; } = new List<DocumentSection>();
}

/// <summary>
/// 문서의 개별 섹션을 나타내는 클래스입니다.
/// 페이지, 슬라이드, 또는 기타 구획을 표현할 수 있습니다.
/// </summary>
public class DocumentSection
{
    /// <summary>
    /// 섹션의 번호를 저장합니다. 보통 순서나 식별에 사용됩니다.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 섹션의 텍스트 내용을 저장합니다.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public DocumentSection() { }

    /// <summary>
    /// 섹션 번호와 텍스트를 초기화하는 생성자입니다.
    /// </summary>
    /// <param name="number">섹션의 번호</param>
    /// <param name="text">섹션의 텍스트 내용</param>
    public DocumentSection(int number, string text)
    {
        Number = number;
        Text = text;
    }
}
