namespace IronHive.Abstractions.Files;

/// <summary>
/// 파일 관련 작업을 수행하는 서비스 입니다.
/// </summary>
public interface IFileStorageManager
{
    /// <summary>
    /// 지정한 저장소 및 접두어(prefix)를 기준으로,
    /// 해당 경로에 존재하는 파일 및 디렉터리 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="storage">스토리지 이름 또는 식별자</param>
    /// <param name="prefix">검색할 경로의 접두어 (선택)</param>
    /// <param name="depth">탐색 깊이 (기본값: 1)</param>
    /// <returns>파일 및 디렉터리 경로 목록</returns>
    Task<IEnumerable<string>> ListAsync(
        string storage,
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일 또는 디렉터리가 존재하는지 비동기적으로 확인합니다.
    /// </summary>
    /// <param name="storage">스토리지 이름 또는 식별자</param>
    /// <param name="path">확인할 파일 또는 디렉터리 경로</param>
    /// <returns>존재하면 true, 그렇지 않으면 false</returns>
    Task<bool> ExistsAsync(
        string storage,
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일을 비동기적으로 읽어 스트림으로 반환합니다.
    /// </summary>
    /// <param name="storage">스토리지 이름 또는 식별자</param>
    /// <param name="filePath">읽을 파일의 경로</param>
    /// <returns>읽은 파일 스트림</returns>
    Task<Stream> ReadFileAsync(
        string storage,
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일을 비동기적으로 작성합니다.
    /// </summary>
    /// <param name="storage">스토리지 이름 또는 식별자</param>
    /// <param name="filePath">작성할 파일 경로</param>
    /// <param name="data">작성할 데이터 스트림</param>
    /// <param name="overwrite">기존 파일을 덮어쓸지 여부 (기본값: true)</param>
    Task WriteFileAsync(
        string storage,
        string filePath,
        Stream data,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일 또는 디렉터리를 비동기적으로 삭제합니다.
    /// </summary>
    /// <param name="storage">스토리지 이름 또는 식별자</param>
    /// <param name="path">삭제할 파일 또는 디렉터리 경로</param>
    Task DeleteAsync(
        string storage,
        string path,
        CancellationToken cancellationToken = default);
}
