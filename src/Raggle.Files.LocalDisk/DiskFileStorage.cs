using Raggle.Abstractions.Memories;

namespace Raggle.Files.LocalDisk;

public class DiskFileStorage : IFileStorage
{
    private readonly string _storageDirectory;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private bool _disposed = false;

    /// <summary>
    /// DiskFileStorage 생성자.
    /// </summary>
    /// <param name="storageDirectory">파일을 저장할 디렉토리 경로.</param>
    public DiskFileStorage(string storageDirectory)
    {
        if (string.IsNullOrWhiteSpace(storageDirectory))
            throw new ArgumentException("Storage directory path cannot be null or whitespace.", nameof(storageDirectory));

        _storageDirectory = storageDirectory;
        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>
    /// 파일을 비동기적으로 저장합니다.
    /// </summary>
    /// <param name="fileName">저장할 파일의 이름.</param>
    /// <param name="content">파일의 내용.</param>
    /// <returns>비동기 작업.</returns>
    public async Task SaveFileAsync(string fileName, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        string filePath = Path.Combine(_storageDirectory, SanitizeFileName(fileName));

        await _semaphore.WaitAsync();
        try
        {
            await File.WriteAllBytesAsync(filePath, content);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 파일을 비동기적으로 읽습니다.
    /// </summary>
    /// <param name="fileName">읽을 파일의 이름.</param>
    /// <returns>파일의 내용.</returns>
    public async Task<byte[]?> ReadFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));

        string filePath = Path.Combine(_storageDirectory, SanitizeFileName(fileName));

        if (!File.Exists(filePath))
            return null;

        await _semaphore.WaitAsync();
        try
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 파일을 비동기적으로 삭제합니다.
    /// </summary>
    /// <param name="fileName">삭제할 파일의 이름.</param>
    /// <returns>비동기 작업.</returns>
    public async Task DeleteFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));

        string filePath = Path.Combine(_storageDirectory, SanitizeFileName(fileName));

        await _semaphore.WaitAsync();
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 저장소에 있는 모든 파일의 이름 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns>파일 이름의 목록.</returns>
    public async Task<IEnumerable<string>> ListFilesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var files = Directory.GetFiles(_storageDirectory)
                                 .Select(f => Path.GetFileName(f))
                                 .ToList();
            return files;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 파일 이름을 안전하게 변환합니다.
    /// </summary>
    /// <param name="fileName">원본 파일 이름.</param>
    /// <returns>안전하게 변환된 파일 이름.</returns>
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    /// <summary>
    /// 세마포어를 동기적으로 기다립니다.
    /// </summary>
    /// <returns>비동기 작업.</returns>
    private void WaitSemaphore()
    {
        _semaphore.Wait();
    }

    /// <summary>
    /// 자원을 해제합니다.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
