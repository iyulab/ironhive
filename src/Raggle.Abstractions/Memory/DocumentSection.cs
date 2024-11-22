namespace Raggle.Abstractions.Memory;

/// <summary>
/// 문서의 개별 섹션을 나타내는 클래스입니다.
/// 페이지, 슬라이드, 또는 기타 구획을 표현할 수 있습니다.
/// </summary>
public class DocumentSection
{
    /// <summary>
    /// 섹션의 식별자를 저장합니다. 예: "1 slide", "2 page"
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// 섹션의 텍스트 내용을 저장합니다.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
