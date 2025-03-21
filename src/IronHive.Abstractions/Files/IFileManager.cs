namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 관련 작업을 수행하는 인터페이스입니다.
/// </summary>
public interface IFileManager
{
    /// <summary>
    /// 파일을 읽고 디코딩하여 텍스트로 반환합니다.
    /// </summary>
    Task<string> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);
}
