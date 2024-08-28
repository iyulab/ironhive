
namespace Raggle.Source;

public class LocalFileSource
{
    private string _baseDirectoryPath;

    public IEnumerable<string> GetFileNames(string index)
    {
        return Directory.GetFiles(_baseDirectoryPath).Select(Path.GetFileName);
    }

    public void UpsertFile(string index, string filename, Stream content)
    {
        CreateDirectory(index, _baseDirectoryPath);
        using (var fileStream = File.Create(Path.Combine(_baseDirectoryPath, filename)))
        {
            content.CopyTo(fileStream);
        }
    }

    public void DeleteFile(string index, string filename)
    {
        File.Delete(Path.Combine(_baseDirectoryPath, filename));
    }

    private void CreateDirectory(string index, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
