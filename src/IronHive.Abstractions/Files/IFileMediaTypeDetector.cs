using System.Diagnostics.CodeAnalysis;

namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 확장자와 MIME 타입 간의 매핑을 제공하는 서비스입니다.
/// </summary>
public interface IFileMediaTypeDetector
{
    /// <summary>
    /// 등록된 모든 확장자 목록입니다.
    /// </summary>
    IEnumerable<string> Extensions { get; }

    /// <summary>
    /// 등록된 모든 MIME 타입 목록입니다.
    /// </summary>
    IEnumerable<string> MediaTypes { get; }

    /// <summary>
    /// 파일 이름 또는 경로에서 확장자를 추출하여 해당하는 MIME 타입을 반환합니다.
    /// </summary>
    /// <param name="fileName">파일 이름 또는 경로</param>
    /// <returns>확장자에 매핑된 MIME 타입. 알 수 없으면 null</returns>
    string? Detect(string fileName);

    /// <summary>
    /// 파일 이름 또는 경로에서 확장자를 추출하여 해당하는 MIME 타입을 반환합니다.
    /// </summary>
    /// <param name="fileName">파일 이름 또는 경로</param>
    /// <param name="mimeType">확장자에 매핑된 MIME 타입. 알 수 없으면 null</param>
    /// <returns>등록된 확장자에 매핑된 MIME 타입이 있으면 true, 그렇지 않으면 false</returns>
    bool TryDetect(string fileName, [MaybeNullWhen(false)] out string mimeType);
}
