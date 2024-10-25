using MessagePack;
using Raggle.Abstractions.Memory;
using Raggle.Document.Azure;
using Raggle.Document.Disk;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

async Task<string> GetKey()
{
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;
    return key;
}



return;