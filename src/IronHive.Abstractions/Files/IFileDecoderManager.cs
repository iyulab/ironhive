namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일들을 디코딩하는 매니저 서비스입니다.
/// </summary>
public interface IFileDecoderManager
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
    /// <param name="filePath">파일 경로 (확장자 확인용)</param>
    /// <param name="data">파일 스트림</param>
    /// <returns>디코딩된 문자열</returns>
    Task<string> DecodeAsync(
        string filePath, 
        Stream data, 
        CancellationToken cancellationToken = default);
}
