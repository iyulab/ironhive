using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Raggle.Stack.WebApi.Configurations;

namespace Raggle.Stack.WebApi.Services;

public class JsonConfigManager : IDisposable
{
    private HiveStackConfig? _config;
    private readonly ILogger<JsonConfigManager> _logger;
    private readonly object _lock = new();
    private Timer? _debounceTimer;
    private bool _disposed = false;

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Gets or sets the options for JSON serialization.
    /// </summary>
    public JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    /// <summary>
    /// Gets the file system watcher.
    /// </summary>
    public FileSystemWatcher? Watcher { get; private set; }

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    public HiveStackConfig? Config
    {
        get
        {
            lock (_lock)
            {
                return _config;
            }
        }
        set
        {
            lock (_lock)
            {
                _config = value;
                if (value != null)
                {
                    Create(value);
                }
                Changed?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<HiveStackConfig?>? Changed;
    public event EventHandler<Exception>? Error;

    public JsonConfigManager(string filePath, ILogger<JsonConfigManager>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        FilePath = Path.GetFullPath(filePath);
        _logger = logger ?? NullLogger<JsonConfigManager>.Instance;

        // 초기 로드
        Config = Load();
        if (Config == null)
        {
            Create(new HiveStackConfig());
            Config = Load() ?? throw new Exception("Failed to create configuration file.");
        }
    }

    public void Watch()
    {
        if (Watcher != null)
        {
            _logger.LogWarning("Watcher is already initialized.");
            return;
        }

        var directory = Path.GetDirectoryName(FilePath);
        var fileName = Path.GetFileName(FilePath);

        if (directory == null || fileName == null)
        {
            throw new ArgumentException("Invalid file path: unable to get directory or file name.");
        }

        Watcher = new FileSystemWatcher
        {
            Path = directory,
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        Watcher.Changed += OnFileChanged;
        Watcher.Created += OnFileChanged;
        Watcher.Deleted += OnFileDeleted;
        Watcher.Renamed += OnFileRenamed;
        Watcher.Error += OnWatcherError;
    }

    public void Stop()
    {
        if (Watcher != null)
        {
            Watcher.Changed -= OnFileChanged;
            Watcher.Created -= OnFileChanged;
            Watcher.Deleted -= OnFileDeleted;
            Watcher.Renamed -= OnFileRenamed;
            Watcher.Error -= OnWatcherError;

            Watcher.EnableRaisingEvents = false;
            Watcher.Dispose();
            Watcher = null;
        }
    }

    public void Move(string newFilePath)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(JsonConfigManager));

        if (string.IsNullOrWhiteSpace(newFilePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(newFilePath));

        try
        {
            Stop();

            var fullNewPath = Path.GetFullPath(newFilePath);
            File.Move(FilePath, fullNewPath);
            _logger.LogInformation("Configuration file moved from {OldPath} to {NewPath}.", FilePath, fullNewPath);
            FilePath = fullNewPath;

            Watch();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move configuration file to {NewPath}.", newFilePath);
            Error?.Invoke(this, ex);
        }
    }

    private void Create(HiveStackConfig config)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(JsonConfigManager));

        try
        {
            var json = JsonSerializer.Serialize(config, Options);
            File.WriteAllText(FilePath, json);
            _logger.LogInformation("Configuration file created at {FilePath}.", FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create configuration file at {FilePath}.", FilePath);
            Error?.Invoke(this, ex);
        }
    }

    private HiveStackConfig? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                _logger.LogWarning("Configuration file not found at {FilePath}.", FilePath);
                return null;
            }

            var json = File.ReadAllText(FilePath);
            var config = JsonSerializer.Deserialize<HiveStackConfig>(json, Options);
            _logger.LogInformation("Configuration loaded from {FilePath}.", FilePath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration from {FilePath}.", FilePath);
            Error?.Invoke(this, ex);
            return null;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // 디바운싱: 500ms 지연 후 로드
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            try
            {
                var newConfig = Load();
                if (newConfig != null)
                {
                    _config = newConfig;
                    Changed?.Invoke(this, newConfig);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file change for {FilePath}.", FilePath);
                Error?.Invoke(this, ex);
            }
        }, null, 500, Timeout.Infinite);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        var exception = new FileNotFoundException("The configuration file was deleted.");
        _logger.LogError(exception, "Configuration file deleted at {FilePath}.", FilePath);
        Error?.Invoke(this, exception);
        Stop();
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        var exception = new FileLoadException("The configuration file was renamed.");
        _logger.LogError(exception, "Configuration file renamed from {OldName} to {NewName}.", e.OldName, e.Name);
        Error?.Invoke(this, exception);
        Stop();
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException() ?? new Exception("Unknown watcher error.");
        _logger.LogError(exception, "An error occurred in the file system watcher for {FilePath}.", FilePath);
        Error?.Invoke(this, exception);
        Stop();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Watcher?.Dispose();
        _debounceTimer?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
