using System.Text.Json;
using Microsoft.AspNetCore.StaticFiles;

async Task<string> GetKey()
{
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;
    return key;
}

var filePath = @"C:\data\Raggle\src\Raggle.ConsoleTests\Program.cs";
var fileInfo = new FileInfo(filePath);
var fileName = fileInfo.Name;

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".cs"] = "text/plain";
if (provider.TryGetContentType(fileName, out var contentType))
{
    Console.WriteLine($"Content type: {contentType}");
}
else
{
    Console.WriteLine("Content type not found");
}

return;
