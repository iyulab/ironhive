using IronHive.Abstractions.Files;
using System.Runtime.InteropServices;

namespace IronHive.Core.Storages;

/// <summary>
/// 현재 머신의 로컬 디스크 파일 시스템을 대상으로 하는 파일 스토리지 구현입니다.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private readonly int _defaultBufferSize = 81_920;  // 기본 80KB
    private readonly int _maxBufferSize = 1_048_576;   // 최대 1MB
    private readonly int _minBufferSize = 4_096;       // 최소 4KB

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        int depth = 1,
        CancellationToken cancellationToken = default)
    {
        prefix = EnsurePath(prefix);

        // Windows의 경우 루트 디렉토리를 드라이브 목록으로 대체 (0 depth only)
        if (_isWindows && prefix == "/")
        {
            var drives = DriveInfo.GetDrives()
                .Select(drive => drive.Name.Replace('\\', '/'));
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(drives);
        }

        var options = new EnumerationOptions
        {
            MatchCasing = MatchCasing.PlatformDefault,                          // 대소문자 구분 플랫폼에 따름
            MatchType = MatchType.Simple,                                       // 단순 일치
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,   // 숨김, 시스템 파일 제외
            IgnoreInaccessible = true,                                          // 접근 불가능한 파일 제외
            ReturnSpecialDirectories = false,                                   // 특수 디렉터리 제외 (prefix ".", "..")
            RecurseSubdirectories = true,                                       // 하위 디렉토리 재귀적 탐색 여부
            MaxRecursionDepth = depth > 0 ? depth : int.MaxValue,               // 재귀적 탐색 깊이
        };

        // 파일 목록을 가져옵니다.
        var files = Directory.GetFiles(prefix, "*", options)
                .Select(file => file.Replace('\\', '/'));
        cancellationToken.ThrowIfCancellationRequested();

        // 폴더 목록을 가져오고, 각 폴더 경로 끝에 구분자를 추가합니다.
        var directories = Directory.GetDirectories(prefix, "*", options)
            .Select(dir => 
            {
                var dic = Path.EndsInDirectorySeparator(dir) ? dir : dir + Path.DirectorySeparatorChar;
                return _isWindows ? dic.Replace('\\', '/') : dic;
            });
        cancellationToken.ThrowIfCancellationRequested();

        // 파일과 폴더를 합칩니다.
        var items = files.Concat(directories);

        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        path = EnsurePath(path);
        var isExists = IsDirectory(path)
            ? Directory.Exists(path)
            : File.Exists(path);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(isExists);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        filePath = EnsurePath(filePath);
        if (!await ExistsAsync(filePath, cancellationToken))
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉토리 경로로 파일을 읽을 수 없습니다.", nameof(filePath));

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _defaultBufferSize, FileOptions.Asynchronous);
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, _defaultBufferSize, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc />
    public async Task WriteFileAsync(
        string filePath,
        Stream data,
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
        filePath = EnsurePath(filePath);
        if (IsDirectory(filePath))
            throw new ArgumentException("디렉토리 경로로 파일을 쓸 수 없습니다.", nameof(filePath));

        // 스트림이 길이 정보를 지원하는 경우 최적 버퍼 크기를 결정
        int bufferSize = data.CanSeek ? GetOptimalBufferSize(data.Length) : _defaultBufferSize;

        // 파일을 쓰기 전에 경로 상의 디렉터리 존재 여부 확인
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(filePath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
        var buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = await data.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        path = EnsurePath(path);
        if (IsDirectory(path))
        {
            // 디렉토리 삭제
            Directory.Delete(path, recursive: true);
        }
        else
        {
            // 파일 삭제
            File.Delete(path);

            // 상위 디렉터리 확인 후, 비어있으면 삭제
            var dicInfo = Directory.GetParent(path);
            if (dicInfo != null && !dicInfo.EnumerateFileSystemInfos().Any())
            {
                Directory.Delete(dicInfo.FullName);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 지정한 경로가 디렉토리 경로인지 확인합니다.
    /// </summary>
    private static bool IsDirectory(string path)
    {
        return Path.EndsInDirectorySeparator(path);
    }

    /// <summary>
    /// 경로를 플랫폼에 맞게 변환합니다.
    /// </summary>
    private string EnsurePath(string? path = null)
    {
        // null 또는 빈 문자열인 경우 "/"를 루트 경로로 반환
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        // Windows의 경우 '/' -> '\'로 변환
        if (_isWindows)
            path = path.Replace('/', '\\');

        // 상대경로의 경우 절대경로로 변환
        //if (!path.StartsWith('/'))
        //    path = '/' + path;

        return path;
    }

    /// <summary>
    /// 스트림 크기에 따라 최적의 버퍼 크기를 반환합니다.
    /// </summary>
    private int GetOptimalBufferSize(long length)
    {
        if (length <= 0) return _defaultBufferSize;

        // 스트림 크기에 따라 동적으로 버퍼 크기 조정
        if (length < 1 * 1024 * 1024) // 1MB 이하
            return _minBufferSize;
        if (length < 100 * 1024 * 1024) // 100MB 이하
            return _defaultBufferSize;

        // 100MB 이상 대용량 파일
        return _maxBufferSize;
    }
}
