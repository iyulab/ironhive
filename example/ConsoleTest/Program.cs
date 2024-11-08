using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Connector.OpenAI;
using Raggle.Core;
using Raggle.Core.Handlers;
using Raggle.Core.Parsers;
using Raggle.DocumentStorage.LocalDisk;
using Raggle.VectorDB.LiteDB;
using System.ComponentModel;
using System.Diagnostics;
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
var documentStorage = new LocalDiskDocumentStorage(new LocalDiskStorageConfig
{
    DirectoryPath = @"C:\temp\document",
});
var vectorStorage = new LiteDBVectorStorage(new LiteDBConfig
{
    DatabasePath = @"C:\temp\vector.db"
});
var chatService = new OpenAIChatCompletionService(await GetKey());
var chatRequest = new ChatCompletionRequest
{ 
    Model= "gpt-4o-mini",
    Messages = [],
    MaxTokens = 2048,
    Temperature = 0.5f,
};
var embeddingService = new OpenAIEmbeddingService(await GetKey());
var embeddingModel = "text-embedding-3-large";

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
    chatRequest: chatRequest));

orchestrator.TryAddHandler("question", new GenerateQAPairsHandler(
    documentStorage: documentStorage,
    chatService: chatService,
    chatRequest: chatRequest));

orchestrator.TryAddHandler("embed", new TextEmbeddingHandler(
    documentStorage: documentStorage,
    vectorStorage: vectorStorage,
    embeddingService: embeddingService,
    embeddingModel: embeddingModel));

var memory = new MemoryService(
   documentStorage: documentStorage,
   vectorStorage: vectorStorage,
   embeddingService: embeddingService,
   embeddingModel: embeddingModel,
   orchestrator: orchestrator);

var collection = "test";
var file = @"C:\temp\sample\word_sample.docx";
var documentId = GetDocumentId(file);
var upload = new DocumentUploadRequest
{
    FileName = Path.GetFileName(file),
    Content = File.OpenRead(file),
};

await memory.CreateCollectionAsync(collection, 3072);
await memory.MemorizeDocumentAsync(
    collectionName: collection,
    documentId: documentId,
    steps: ["parse", "chunk", "summarize", "question", "embed"],
    uploadRequest: upload);

var timer = new Stopwatch();
timer.Start();
await memory.SearchDocumentMemoryAsync(
    collectionName: collection,
    query: "What is somatosensory system?",
    minScore: 0.5f,
    limit: 5);
timer.Stop();
Console.WriteLine($"Elapsed time: {timer.ElapsedMilliseconds} ms");

var provider = new RaggleServiceProvider();


return;

string GetDocumentId(string filename)
{
    var bytes = Encoding.UTF8.GetBytes(filename);
    var hash = MD5.Create().ComputeHash(bytes);
    return BitConverter.ToString(hash).Replace("-", "");
}

void PrintJson(object obj)
{
    Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

public class MyWeatherTool
{
    [FunctionTool("GetWeather")]
    [Description("Get the weather of a city")]
    public string GetWeather(
        [Description("The city name")] string city) // "Seoul
    {
        return $"The weather in {city} is sunny";
    }
}
