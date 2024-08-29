using Raggle.Console.Settings;
using System.Text.Json;

namespace Raggle.Console;

public class AppBuilder : IDisposable
{
    private readonly string _baseDir;
    private readonly string _configDir;
    private readonly string _settingsPath;

    public AppBuilder(string baseDir)
    {
        _baseDir = baseDir;
        _configDir = Path.Combine(_baseDir, Constants.SETTING_DIRECTORY);
        _settingsPath = Path.Combine(_configDir, Constants.SETTING_FILENAME);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void SaveSettings(AppSettings settings)
    {
        if (!Directory.Exists(_configDir))
        {
            Directory.CreateDirectory(_configDir);
            File.SetAttributes(_configDir, File.GetAttributes(_configDir) | FileAttributes.Hidden);
        }

        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public void DeleteConfig()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }

    public AppSettings? GetSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return null;
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
