using Raggle.Engines.Anthropic;
using System.Text.Json;

var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;


