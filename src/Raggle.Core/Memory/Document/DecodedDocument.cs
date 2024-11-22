using Raggle.Abstractions.Memory;

namespace Raggle.Core.Memory.Document;

/// <summary>
/// 문서의 전체 정보를 나타내는 클래스입니다.
/// </summary>
public class DecodedDocument
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
    public IEnumerable<DocumentSection> Sections { get; set; } = [];
}
