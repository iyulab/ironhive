namespace Raggle.Server.Web.Storages;

public class FileStorage
{
    private readonly string _baseDirectory;

    public FileStorage(IConfiguration config)
    {
        _baseDirectory = config.GetSection("FileStorage:Path").Value;
    }

    public async Task UploadAsync(string index, string fileName, Stream stream)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await stream.CopyToAsync(fileStream);
    }

    public Stream Download(string index, string fileName)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.");
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public IEnumerable<UploadedFile> GetUploadedFiles(string index)
    {
        var paths = index.Split('/');
        var directoryPath = Path.Combine(_baseDirectory, Path.Combine(paths));
        if (!Directory.Exists(directoryPath))
        {
            throw new FileNotFoundException("Directory not found.");
        }

        var files = Directory.GetFiles(directoryPath);
        var uploadedFiles = new List<UploadedFile>();
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            uploadedFiles.Add(new UploadedFile { FileName = fileName, Content = fileStream });
        }

        return uploadedFiles;
    }

    public void DeleteFile(string index, string fileName)
    {
        var paths = index.Split('/');
        var filePath = Path.Combine(_baseDirectory, Path.Combine(paths), fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.");
        }

        File.Delete(filePath);
    }

    public void DeleteDirectory(string index)
    {
        var paths = index.Split('/');
        var directoryPath = Path.Combine(_baseDirectory, Path.Combine(paths));
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException("Directory not found.");
        }

        Directory.Delete(directoryPath, true);
    }
}

public class UploadedFile
{
    public required string FileName { get; set; }
    public required Stream Content { get; set; }
}