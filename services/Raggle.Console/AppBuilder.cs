using Raggle.Abstractions;
using Raggle.Console.Settings;
using Raggle.Core;
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

    public IRaggleService BuildRaggleService(AppSettings settings)
    {
        var builder = new RaggleServiceBuilder();
        var platformType = settings.Platforms.Type;
        if (platformType == PlatformTypes.OpenAI)
        {
            builder.WithOpenAI(settings.Platforms.OpenAI);
        }

        var vectorType = settings.VectorDB.Type;
        if (vectorType == VectorDBTypes.File)
        {
            var vectorOption = settings.VectorDB.FileVectorDB;
            vectorOption.ChunkDirectory = Path.Combine(_configDir, Constants.FILES_DIRECTORY);
            vectorOption.VectorDirectory = Path.Combine(_configDir, Constants.VECTOR_DIRECTORY);
            builder.WithFileVectorDB(vectorOption);
        }

        var promptType = settings.Prompts.Type;
        if (promptType == PromptTypes.Simple)
        {
            var promptOption = settings.Prompts.SimplePrompt;
            builder.WithSimplePrompt(promptOption);
        }

        return builder.Build();
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
