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
    public DiskFileStorage(string dirPath)
    {
        if (string.IsNullOrWhiteSpace(dirPath))
            throw new ArgumentException("Storage directory path cannot be null or whitespace.", nameof(dirPath));
        _storageDirectory = dirPath;
        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>
    /// 파일을 비동기적으로 저장합니다.
    /// </summary>
    /// <param name="filePath">저장할 파일의 경로</param>
    /// <param name="content">파일의 내용.</param>
    /// <returns>비동기 작업.</returns>
    public async Task SaveFileAsync(string filePath, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        filePath = Path.Combine(_storageDirectory, filePath);
        
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
    /// <param name="filePath">읽을 파일의 경로</param>
    /// <returns>파일의 내용.</returns>
    public async Task<byte[]?> ReadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        filePath = Path.Combine(_storageDirectory, filePath);

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
    /// <param name="filePath">삭제할 파일의 이름.</param>
    /// <returns>비동기 작업.</returns>
    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        filePath = Path.Combine(_storageDirectory, filePath);

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
    /// 저장소에 있는 모든 파일의 목록을 비동기적으로 가져옵니다.
    /// </summary>
    public async Task<IEnumerable<string>> ListFilesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var files = Directory.GetFiles(_storageDirectory)
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
