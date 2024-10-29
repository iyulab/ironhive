using Raggle.Abstractions.Memory.Document;
using Raggle.Core;
using Raggle.Core.Handlers;
using Raggle.Core.Parsers;
using Raggle.Document.Disk;
using Raggle.Vector.LiteDB;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

async Task<string> GetKey()
{
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;
    return key;
}
var documentStorage = new DiskDocumentStorage(new DiskStorageConfig
{
    DirectoryPath = @"C:\temp\document",
});
var vectorStorage = new LiteDBVectorStorage(new LiteDBConfig
{ 
    DatabasePath = @"C:\temp\vector.db"
});

var orchestrator = new PipelineOrchestrator(
    documentStorage: documentStorage);

orchestrator.TryAddHandler("parse", new DocumentParsingHandler(
    documentStorage: documentStorage,
    [
        new PdfParser(),
        new MSWordParser(),
        new TextPlainParser(),
        new MSPresentationParser(),
    ]));

orchestrator.TryAddHandler("chunk", new DocumentChunkingHandler(
    documentStorage: documentStorage,
    maxTokensPerChunk: 1024));

var memory = new MemoryService(
   documentStorage: documentStorage,
   vectorStorage: vectorStorage,
   orchestrator: orchestrator);

var collection = "test";
var file = @"C:\temp\sample\ppt_sample.pptx";
var upload = new UploadRequest
{
    FileName = Path.GetFileName(file),
    Content = File.OpenRead(file),
};

await memory.CreateCollectionAsync(collection, 512);
await memory.MemorizeDocumentAsync(
    collectionName: collection,
    documentId: GetDocumentId(file),
    steps: ["parse", "chunk"],
    uploadRequest: upload);

return;

string GetDocumentId(string filename)
{
    var bytes = Encoding.UTF8.GetBytes(filename);
    var hash = MD5.Create().ComputeHash(bytes);
    return BitConverter.ToString(hash).Replace("-", "");
}