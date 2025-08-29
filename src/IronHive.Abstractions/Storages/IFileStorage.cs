namespace IronHive.Abstractions.Storages;

/// <summary>
/// 파일 저장소 연결을 위한 인터페이스입니다.
/// </summary>
public interface IFileStorage : IDisposable
{
    /// <summary>
    /// 저장소의 이름을 반환합니다.
    /// </summary>
    string StorageName { get; }

    /// <summary>
    /// 지정한 접두어(prefix)를 가진 폴더 내의 파일 및 디렉토리의 목록을 반환합니다.
    /// 디렉토리는 '/'로 끝나는 문자열로 반환됩니다.
    /// (하위 폴더의 내용은 포함되지 않습니다.)
    /// </summary>
    /// <param name="prefix">
    /// 나열하기 원하는 파일 또는 디렉토리 경로의 접두어입니다. null 또는 빈 문자열일 경우, 루트 항목을 나열합니다.
    /// </param>
    /// <param name="depth">
    /// 탐색하기 원하는 디렉토리의 깊이입니다. 기본값은 1입니다. 0 이하는 모든 하위 디렉토리를 탐색합니다.
    /// </param>
    /// <returns>탐색 결과로 파일 및 디렉토리 경로의 목록을 반환합니다.</returns>
    Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일 또는 디렉토리가 존재하는지 비동기적으로 확인합니다.
    /// </summary>
    /// <param name="path">확인할 파일 또는 디렉토리의 경로입니다.</param>
    /// <returns>존재 여부를 나타내는 <see cref="Task{bool}"/>를 반환합니다.</returns>
    Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일을 비동기적으로 읽어들입니다.
    /// </summary>
    /// <param name="filePath">읽을 파일의 경로입니다.</param>
    /// <returns>내용을 포함하는 <see cref="Task{Stream}"/>를 반환합니다.</returns>
    Task<Stream> ReadFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로에 파일을 비동기적으로 작성합니다.
    /// </summary>
    /// <param name="filePath">파일을 저장할 경로입니다.</param>
    /// <param name="data">쓰기 위한 데이터 스트림입니다.</param>
    /// <param name="overwrite">
    /// 파일이 이미 존재하는 경우 덮어쓸지 여부입니다. 기본값은 true입니다.
    /// </param>
    Task WriteFileAsync(
        string filePath,
        Stream data,
        bool overwrite = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정한 경로의 파일 또는 디렉토리를 비동기적으로 삭제합니다.
    /// 디렉토리 경로의 경우, 하위 디렉토리 및 파일도 모두 삭제됩니다.
    /// </summary>
    /// <param name="path">삭제할 파일 또는 디렉토리의 경로입니다.</param>
    Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default);
}
