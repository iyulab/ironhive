using Raggle.Server.Configurations;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Raggle.Server.Services;

public class ConfigurationService
{
    private const string DefaultFileName = "raggle_settings.json";
    private const string SectionName = "Raggle";

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();
    private readonly FileSystemWatcher _watcher;

    private RaggleConfig _config;

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RaggleConfigManager"/> class.
    /// </summary>
    /// <param name="filePath">Optional path to the configuration file. If null, the default path is used.</param>
    /// <param name="jsonOptions">Optional JSON serializer options. If null, default options are used.</param>
    public ConfigurationService(string? filePath = null, JsonSerializerOptions? jsonOptions = null)
    {
        FilePath = filePath ?? GetDefaultPath();
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } // Changed to CamelCase for standardization
        };

        _watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(FilePath) ?? throw new ArgumentException("Invalid file path: unable to get directory path."),
            Filter = Path.GetFileName(FilePath) ?? throw new ArgumentException("Invalid file path: unable to get file name."),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };

        _watcher.Changed += OnConfigFileChanged;
        _watcher.Created += OnConfigFileChanged;
        _watcher.Deleted += OnConfigFileDeleted;
        _watcher.Renamed += OnConfigFileRenamed;
        _watcher.EnableRaisingEvents = true;

        LoadConfig(); // Initial load
    }

    /// <summary>
    /// Retrieves the current configuration.
    /// </summary>
    /// <returns>The current <see cref="RaggleConfig"/>.</returns>
    public RaggleConfig GetConfig()
    {
        lock (_lock)
        {
            return _config;
        }
    }

    /// <summary>
    /// Sets a new file path for the configuration file and reloads the configuration.
    /// </summary>
    /// <param name="filePath">The new file path.</param>
    public void SetFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        lock (_lock)
        {
            // Disable current watcher
            _watcher.EnableRaisingEvents = false;

            FilePath = filePath;
            ConfigureWatcher();

            LoadConfig(); // Reload configuration with the new file path

            // Re-enable watcher
            _watcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>
    /// Saves the current configuration to the file.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    public void SaveConfig(RaggleConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        lock (_lock)
        {
            var wrapper = new { Raggle = config };
            var jsonText = JsonSerializer.Serialize(wrapper, _jsonOptions);
            File.WriteAllText(FilePath, jsonText);
            _config = config; // Update the in-memory config
        }
    }

    #region Private Methods

    /// <summary>
    /// Loads the configuration from the file.
    /// </summary>
    private void LoadConfig()
    {
        //lock (_lock)
        //{
        //    if (!File.Exists(FilePath))
        //    {
        //        throw new FileNotFoundException($"Configuration file not found at path: {FilePath}");
        //    }

        //    try
        //    {
        //        var jsonText = File.ReadAllText(FilePath);
        //        using var jsonDoc = JsonDocument.Parse(jsonText, _jsonOptions);
        //        if (!jsonDoc.RootElement.TryGetProperty(SectionName, out var raggleElement))
        //        {
        //            throw new JsonException($"Section '{SectionName}' not found in configuration.");
        //        }

        //        _config = raggleElement.Deserialize<RaggleConfig>(_jsonOptions)
        //                  ?? throw new JsonException("Failed to deserialize Raggle configuration.");
        //    }
        //    catch (Exception ex) when (ex is IOException || ex is JsonException)
        //    {
        //        // Handle exceptions related to file access or JSON parsing
        //        // You might want to log these exceptions or rethrow them
        //        throw new InvalidOperationException("Failed to load configuration.", ex);
        //    }
        //}
    }

    /// <summary>
    /// Configures the FileSystemWatcher with the current FilePath.
    /// </summary>
    private void ConfigureWatcher()
    {
        var directoryPath = Path.GetDirectoryName(FilePath)
                            ?? throw new ArgumentException("Invalid file path: unable to get directory path.");
        var fileName = Path.GetFileName(FilePath)
                       ?? throw new ArgumentException("Invalid file path: unable to get file name.");

        _watcher.Path = directoryPath;
        _watcher.Filter = fileName;
    }

    /// <summary>
    /// Gets the default path for the configuration file.
    /// </summary>
    /// <returns>The default file path.</returns>
    private string GetDefaultPath()
    {
        return Path.Combine(AppContext.BaseDirectory, DefaultFileName);
    }

    /// <summary>
    /// Handles changes to the configuration file.
    /// </summary>
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce the event to avoid multiple triggers
        Thread.Sleep(100); // Simple debounce; consider more robust solutions for production

        try
        {
            LoadConfig();
            // Optionally, notify other parts of the application about the config change
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error)
            // Decide whether to rethrow or swallow the exception based on your needs
        }
    }

    /// <summary>
    /// Handles deletion of the configuration file.
    /// </summary>
    private void OnConfigFileDeleted(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _config = default;
            // Optionally, notify other parts of the application about the config deletion
        }
    }

    /// <summary>
    /// Handles renaming of the configuration file.
    /// </summary>
    private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        lock (_lock)
        {
            FilePath = e.FullPath;
            ConfigureWatcher();
            LoadConfig();
            // Optionally, notify other parts of the application about the config rename
        }
    }

    #endregion
}
