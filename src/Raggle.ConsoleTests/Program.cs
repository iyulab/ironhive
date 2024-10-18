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

var pipe = new DataPipeline
{
    CollectionName = "Test",
    DocumentId = "Test",
    Steps = ["1","2","3","4","5"],
};
pipe.InitializeSteps();
pipe.AdjustSteps(3);

var json = JsonSerializer.Serialize(pipe);

var pipe2 = JsonSerializer.Deserialize<DataPipeline>(json);

return;