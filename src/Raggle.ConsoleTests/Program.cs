using Raggle.Core.Decoders;
using System.Text.Json;

async Task<string> GetKey()
{
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;
    return key;
}

var filePath = @"C:\temp\sample\ppt_sample.pptx";
using var stream = File.OpenRead(filePath);

var decoder = new SlideDecoder();
await decoder.DecodeAsync(stream);

return;