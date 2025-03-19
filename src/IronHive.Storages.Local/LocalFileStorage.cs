using IronHive.Abstractions.Memory;
using System.Runtime.InteropServices;

namespace IronHive.Storages.LocalDisk;

public class LocalFileStorage : IFileStorage
{
    private const int _defaultBufferSize = 81_920; // 기본 80KB
    private const int _maxBufferSize = 1_048_576;   // 최대 1MB
    private const int _minBufferSize = 4_096;      // 최소 4KB

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            // 플랫폼에 따른 처리: Windows면 드라이브 목록, 그 외는 루트만 반환
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var items = DriveInfo.GetDrives().Select(drive => drive.Name);
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(items);
            }
            else
            {
                // Linux/Unix 환경에서는 루트("/")만 반환합니다.
                var items = (IEnumerable<string>)new[] { "/" };
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(items);
            }
        }
        else
        {
            var options = new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,   // 숨김, 시스템 파일 제외
                RecurseSubdirectories = false,                                      // 1depth만 탐색
                IgnoreInaccessible = true,                                          // 접근 불가능한 파일 제외
                MatchCasing = MatchCasing.PlatformDefault,                          // 플랫폼 기본 대소문자 구분
                MatchType = MatchType.Simple,                                       // 단순 일치
                ReturnSpecialDirectories = false,                                   // 특수 디렉터리 제외
            };

            // 파일 목록을 가져옵니다.
            var files = Directory.GetFiles(prefix, "*", options);
            cancellationToken.ThrowIfCancellationRequested();

            // 폴더 목록을 가져오고, 각 폴더 경로 끝에 구분자를 추가합니다.
            var directories = Directory.GetDirectories(prefix, "*", options)
                .Select(dir => Path.EndsInDirectorySeparator(dir) ? dir : dir + Path.DirectorySeparatorChar);
            cancellationToken.ThrowIfCancellationRequested();

            // 파일과 폴더를 합칩니다.
            var items = files.Concat(directories);

            return Task.FromResult(items);
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        var isExists = IsDirectory(path)
            ? Directory.Exists(path)
            : File.Exists(path);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(isExists);
    }

    /// <inheritdoc />
    public async Task<Stream> ReadAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
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
    public async Task WriteAsync(
        string filePath,
        Stream data,
        bool overwrite = true, 
        CancellationToken cancellationToken = default)
    {
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
        if (IsDirectory(path))
            Directory.Delete(path, true);
        else
            File.Delete(path);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 지정한 경로가 디렉토리 경로인지 확인합니다.
    /// </summary>
    public static bool IsDirectory(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar);
    }

    // 스트림 크기에 따라 최적의 버퍼 크기를 반환합니다.
    private static int GetOptimalBufferSize(long length)
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
