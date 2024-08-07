using NRedisStack.Search;
using System.Threading.Tasks;

namespace Raggle.Server.API.Storages;

public class FileStorage
{
    private readonly string _baseDirectory;

    public FileStorage(IConfiguration config)
    {
        _baseDirectory = config.GetSection("FileStorage:Path").Value;
    }

    public async Task UploadFile(string index, string fileName, Stream stream)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);
    }

    public async Task<byte[]> DownloadFile(string index, string fileName)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.");
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<string[]> GetFileNames(string index)
    {
        var paths = index.Split('/');
        var directoryPath = Path.Combine(_baseDirectory, Path.Combine(paths));

        if (!File.Exists(directoryPath))
        {
            throw new FileNotFoundException("File not found.");
        }

        var files = Directory.GetFiles(directoryPath);
        return files.Select(Path.GetFileName).ToArray();
    }

    public async Task<bool> DeleteFile(string index, string fileName)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.");
        }

        File.Delete(filePath);

        return true;
    }
}
