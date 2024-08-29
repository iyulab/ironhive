using Spectre.Console;
using System.Security.Cryptography;
using System.Text;

namespace Raggle.Console.Systems;

public class FileSystem
{
    private readonly FileSystemWatcher _watcher = new();

    public FileSystem()
    {
    }

    public async Task Initialize(string baseDir)
    {
        AnsiConsole.MarkupLine($"[bold green]Initializing base directory: {baseDir} [/]");
        var files = CollectFiles(baseDir);
        await MemorizeFilesAsync(files);
    }

    public void Watch(string directory)
    {
        _watcher.Path = directory;
        _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        _watcher.Filter = "*.*";
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Task.Run(async () =>
        {
            if (IsSetting(e.FullPath)) return;
            await MemorizeFileAsync(e.FullPath);
        });
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Task.Run(async () =>
        {
            if (IsSetting(e.FullPath)) return;
            await UnMemorizeFileAsync(e.FullPath);
            await MemorizeFileAsync(e.FullPath);
        });
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Task.Run(async () =>
        {
            if (IsSetting(e.FullPath)) return;
            await MemorizeFileAsync(e.FullPath);
            await UnMemorizeFileAsync(e.OldFullPath);
        });
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Task.Run(async () =>
        {
            if (IsSetting(e.FullPath)) return;
            await UnMemorizeFileAsync(e.FullPath);
        });
    }

    private bool IsSetting(string path)
    {
        return path.Contains(Constants.SETTING_DIRECTORY);
    }

    private IEnumerable<string> CollectFiles(string baseDir)
    {
        var files = new List<string>();

        string[] baseFiles = Directory.GetFiles(baseDir);
        files.AddRange(baseFiles);

        string[] subDirectories = Directory.GetDirectories(baseDir);
        foreach (string subDir in subDirectories)
        {
            if (Path.GetFileName(subDir) == Constants.SETTING_DIRECTORY)
            {
                continue;
            }
            files.AddRange(CollectFiles(subDir));
        }

        return files;
    }

    private async Task MemorizeFileAsync(string path)
    {
        var documentId = GenerateDocumentId(path);
    }

    private async Task MemorizeFilesAsync(IEnumerable<string> paths)
    {
        await Task.WhenAll(paths.Select(MemorizeFileAsync));
    }

    private async Task UnMemorizeFileAsync(string path)
    {
        var documentId = GenerateDocumentId(path);
    }

    private async Task UnMemorizeFilesAsync(IEnumerable<string> paths)
    {
        await Task.WhenAll(paths.Select(UnMemorizeFileAsync));
    }

    private string GenerateDocumentId(string cotent)
    {
        var encryption = SHA256.HashData(Encoding.UTF8.GetBytes(cotent));
        return Convert.ToHexString(encryption).ToUpperInvariant();
    }
}
