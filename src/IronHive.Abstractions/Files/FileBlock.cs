using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 파싱 결과의 단위 블록입니다. 하나의 파일은 복수의 블록으로 구성될 수 있습니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextBlock), "text")]
[JsonDerivedType(typeof(ImageBlock), "image")]
[JsonDerivedType(typeof(BinaryBlock), "binary")]
public abstract class FileBlock { }

/// <summary>
/// 추출된 텍스트 블록입니다. PDF 페이지, 슬라이드, 시트 등 구획별로 분리됩니다.
/// </summary>
public class TextBlock : FileBlock
{
    /// <summary>출처 구획 이름 (예: "report.pdf - Page 1", "data.xlsx - Sheet1")</summary>
    public string? Label { get; set; }

    /// <summary>정제된 텍스트 내용</summary>
    public required string Text { get; set; }
}

/// <summary>
/// 이미지 블록입니다. 독립 이미지 파일이거나 문서 내 삽입 이미지입니다.
/// </summary>
public class ImageBlock : FileBlock
{
    /// <summary>이미지 MIME 타입 (예: "image/png", "image/jpeg")</summary>
    public required string MimeType { get; set; }

    /// <summary>이미지 원본 바이트</summary>
    public required byte[] Data { get; set; }
}

/// <summary>
/// 해석 불가 바이너리 블록입니다. 파서가 없고 null byte가 감지된 파일에 사용됩니다.
/// </summary>
public class BinaryBlock : FileBlock
{
    /// <summary>원본 바이트</summary>
    public required byte[] Data { get; set; }
}
