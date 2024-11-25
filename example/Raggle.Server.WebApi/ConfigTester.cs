using Raggle.Server.WebApi.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi;

public static class ConfigTester
{
    public static RaggleConfig Config(bool init = false)
    {
        //var baseDir = AppContext.BaseDirectory;
        var baseDir = Directory.GetCurrentDirectory();
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
        };
        var filePath = Path.Combine(baseDir, "raggle_settings.json");

        if (!init && File.Exists(filePath))
        {
            var jsonText = File.ReadAllText(filePath);
            var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(jsonText, jsonOptions);
            var config = jsonDoc?.RootElement.GetProperty("Raggle").Deserialize<RaggleConfig>(jsonOptions);
            return config;
        }
        else
        {
            var config = new RaggleConfig();
            var jsonConfig = new { Raggle = config };
            var jsonText = JsonSerializer.Serialize(jsonConfig, jsonOptions);
            File.WriteAllText(filePath, jsonText);
            return config;
        }
    }
}
