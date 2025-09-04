namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일의 내용을 추출(디코딩)하는 서비스를 정의하는 인터페이스입니다.
/// </summary>
public interface IFileExtractionService<T>
{
    /// <summary>
    /// 등록된 디코더들이 지원하는 파일 확장자 목록을 반환
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }

    /// <summary>
    /// 등록된 디코더들이 지원하는 MIME 타입 목록을 반환
    /// </summary>
    IEnumerable<string> SupportedMimeTypes { get; }

    /// <summary>
    /// 지정된 파일 스트림을 디코딩
    /// </summary>
    /// <param name="fileName">파일 경로 또는 이름 (확장자 확인용)</param>
    /// <param name="data">파일 스트림</param>
    /// <returns>디코딩된 문자열</returns>
    Task<T> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);
}
