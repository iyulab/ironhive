namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 디코더 관리 인터페이스
/// </summary>
public interface IFileDecoderManager
{
    /// <summary>
    /// 등록된 디코더들이 지원하는 파일 확장자 목록을 반환
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }

    /// <summary>
    /// 파일 경로에서 확장자를 추출하여 MimeType 반환
    /// </summary>
    /// <param name="filePath">확장자를 포함하는 파일 경로 또는 파일이름</param>
    /// <returns>등록된 MimeType을 반환, 없으면 null</returns>
    string? GetMimeType(string filePath);

    /// <summary>
    /// 확장자에 대한 MimeType 매핑을 추가하거나 수정
    /// </summary>
    /// <param name="extension">파일 확장자 (예: .txt)</param>
    /// <param name="mimeType">MimeType (예: text/plain)</param>
    void SetMimeType(string extension, string mimeType);

    /// <summary>
    /// 지정된 파일 스트림을 디코딩
    /// </summary>
    /// <param name="filePath">파일 경로 (확장자 확인용)</param>
    /// <param name="data">파일 스트림</param>
    /// <returns>디코딩된 문자열</returns>
    Task<string> DecodeAsync(
        string filePath, 
        Stream data, 
        CancellationToken cancellationToken = default);
}
