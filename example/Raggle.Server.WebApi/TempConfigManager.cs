using Raggle.Server.Configurations;
using Raggle.Server.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi;

public static class TempConfigManager
{
    public static RaggleConfig Make(bool init = false)
    {
        //var baseDir = AppContext.BaseDirectory;
        var baseDir = Directory.GetCurrentDirectory();
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { 
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
                new ServiceKeyValueJsonConverterFactory()
            }
        };
        var filePath = Path.Combine(baseDir, "raggle_settings.json");

        if (!init && File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<RaggleConfig>(json, jsonOptions);
            return config;
        }
        else
        {
            var config = new RaggleConfig();
            var json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(filePath, json);
            return config;
        }
    }
}
