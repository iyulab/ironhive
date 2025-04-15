namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 관련 작업을 수행하는 인터페이스입니다.
/// </summary>
public interface IFileStorageManager
{
    /// <summary>
    /// 지정한 파일 저장소 연결을 생성합니다.
    /// </summary>
    /// <param name="provider">스토리지 등록시 사용한 serviceKey입니다.</param>
    /// <param name="providerConfig">스토리지 생성을 위한 설정용 오브젝트 객체입니다.</param>
    /// <returns>파일 저장소 객체입니다.</returns>
    IFileStorage CreateFileStorage(
        string provider, 
        object? providerConfig = null);

    /// <summary>
    /// 지정한 접두어(prefix)를 가진 폴더 내의 파일 및 디렉토리의 목록을 반환합니다.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> ListAsync(
        string provider,
        string? prefix = null,
        int depth = 1,
        object? providerConfig = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일 또는 디렉토리가 존재하는지 비동기적으로 확인합니다.
    /// </summary>
    Task<bool> ExistsAsync(
        string provider, 
        string path, 
        object? providerConfig = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일을 비동기적으로 읽어들입니다.
    /// </summary>
    Task<Stream> ReadFileAsync(
        string provider,
        string filePath, 
        object? providerConfig = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일을 비동기적으로 작성합니다.
    /// </summary>
    Task WriteFileAsync(
        string provider, 
        string filePath, 
        Stream data, 
        bool overwrite = true, 
        object? providerConfig = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일 또는 디렉토리를 비동기적으로 삭제합니다.
    /// </summary>
    Task DeleteAsync(
        string provider, 
        string path, 
        object? providerConfig = null, 
        CancellationToken cancellationToken = default);
}
