using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory.Document;
using Raggle.Connector.OpenAI;
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
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\example\ConsoleTest\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("OpenAI") ?? string.Empty;
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
var chatService = new OpenAIChatCompletionService(await GetKey());
var chatOption = new ChatCompletionOptions
{ 
    ModelId= "gpt-4o-mini",
    MaxTokens = 2048,
    Temperature = 0.5f,
};
var embeddingService = new OpenAIEmbeddingService(await GetKey());
var embeddingOption = new EmbeddingOptions
{
    ModelId = "text-embedding-3-large",
};

var orchestrator = new PipelineOrchestrator(documentStorage: documentStorage);

orchestrator.TryAddHandler("parse", new DocumentParsingHandler(
    documentStorage: documentStorage,
    [
        new PDFParser(),
        new WordParser(),
        new TextParser(),
        new PPTParser(),
    ]));

orchestrator.TryAddHandler("chunk", new TextChunkingHandler(
    documentStorage: documentStorage,
    maxTokensPerChunk: 1024));

orchestrator.TryAddHandler("summarize", new GenerateSummarizedTextHandler(
    documentStorage: documentStorage,
    chatService: chatService,
    chatOptions: chatOption));

orchestrator.TryAddHandler("question", new GenerateQAPairsHandler(
    documentStorage: documentStorage,
    chatService: chatService,
    chatOptions: chatOption));

orchestrator.TryAddHandler("embed", new TextEmbeddingHandler(
    documentStorage: documentStorage,
    vectorStorage: vectorStorage,
    embeddingService: embeddingService,
    embeddingOptions: embeddingOption));

var memory = new MemoryService(
   documentStorage: documentStorage,
   vectorStorage: vectorStorage,
   orchestrator: orchestrator);

var collection = "test";
var file = @"C:\temp\sample\word_sample.docx";
var upload = new UploadRequest
{
    FileName = Path.GetFileName(file),
    Content = File.OpenRead(file),
};

//await memory.CreateCollectionAsync(collection, 3072);
await memory.MemorizeDocumentAsync(
    collectionName: collection,
    documentId: GetDocumentId(file),
    steps: ["parse", "chunk", "summarize", "question", "embed"],
    uploadRequest: upload);

return;

string GetDocumentId(string filename)
{
    var bytes = Encoding.UTF8.GetBytes(filename);
    var hash = MD5.Create().ComputeHash(bytes);
    return BitConverter.ToString(hash).Replace("-", "");
}