using IronHive.Abstractions.Memory;

namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 관련 작업을 수행하는 인터페이스입니다.
/// </summary>
public interface IFileStorageManager
{
    /// <summary>
    /// 파일 스토리지를 생성합니다.
    /// </summary>
    IFileStorage CreateStorage(string provider, object? config);

    /// <summary>
    /// 파일을 읽고 디코딩하여 텍스트로 반환합니다.
    /// </summary>
    Task<string> DecodeAsync(
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파일 메모리 소스를 읽고 디코딩하여 텍스트로 반환합니다.
    /// </summary>
    Task<string> DecodeAsync(
        FileMemorySource source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 파일의 MIME 타입을 반환합니다.
    /// </summary>
    string GetMimeType(
        string fileName,
        CancellationToken cancellationToken = default);
}
