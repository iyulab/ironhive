using ConsoleTest;
using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Files;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Core;
using IronHive.Core.Handlers;
using IronHive.Core.Storages;
using IronHive.Storages.Qdrant;
using IronHive.Storages.RabbitMQ;
using IronHive.Storages.Redis;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var text = "Hello, World!";
Console.WriteLine(text);
var coll = "test";

var memory = Create();
//var queue = new RabbitMQueueStorage(new RabbitMQConfig());

//await queue.CreateQueueAsync("test");
//await queue.CreateQueueAsync("test2");
//await queue.CreateQueueAsync("test3");

//var queues = await queue.ListQueuesAsync();
//foreach (var q in queues)
//{
//    Console.WriteLine(q);
//}

//Console.WriteLine(await queue.ExistsQueueAsync("test"));
//Console.WriteLine(await queue.ExistsQueueAsync("test2"));
//Console.WriteLine(await queue.ExistsQueueAsync("test3"));
//Console.WriteLine(await queue.ExistsQueueAsync("test4"));

//await queue.DeleteQueueAsync("test");
//await queue.DeleteQueueAsync("test2");
//await queue.DeleteQueueAsync("test3");

_ = memory.StartWorkerAsync();

//await memory.CreateCollectionAsync(coll, "openai", "text-embedding-3-large");

//await memory.MemorizeAsync(coll, new FileMemorySource
//{
//    Provider = "local",
//    Id = Guid.NewGuid().ToString(),
//    FilePath = "C:\\data\\ironhive\\test\\Resources\\pdf-sample.pdf",
//},
//[
//    "decode",
//    "chunk",
//    "qnagen",
//    "embed",
//],
//handlerOptions: new Dictionary<string, object>
//{
//    { "decode", null },
//    { "chunk", new ChunkHandler.Options
//    { 
//        ChunkSize = 8_000,
//    }},
//    { "qnagen", new QnAGenHandler.Options
//    {
//        Provider = "openai",
//        Model = "gpt-4o-mini",
//    }},
//    { "embed", new EmbedHandler.Options
//    {
//        Collection = coll,
//        Provider = "openai",
//        Model = "text-embedding-3-large",
//    }},
//});

//await Task.Delay(60_000 * 5);

var docs = await memory.SearchAsync(coll, "openai", "text-embedding-3-large", "Hello, World!");

await memory.DeleteCollectionAsync(coll);

return;

IHiveMemory Create()
{
    var o_config = new OpenAIConfig
    {
        ApiKey = "",
    };
    var a_config = new AnthropicConfig
    {
        ApiKey = "",
    };
    var g_config = new OpenAIConfig
    {
        BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
        ApiKey = ""
    };
    var l_config = new OpenAIConfig
    {
        BaseUrl = "http://172.30.1.53:8080/v1-openai/",
        ApiKey = ""
    };

    var memory = new HiveServiceBuilder()
        .AddChatCompletionConnector("openai", new OpenAIChatCompletionConnector(o_config))
        .AddEmbeddingConnector("openai", new OpenAIEmbeddingConnector(o_config))
        .AddChatCompletionConnector("anthropic", new AnthropicChatCompletionConnector(a_config))
        .AddChatCompletionConnector("gemini", new OpenAIChatCompletionConnector(g_config))
        .AddChatCompletionConnector("iyulab", new OpenAIChatCompletionConnector(l_config))
        .AddFileStorage("local", (sp, config) => new LocalFileStorage())
        .WithVectorStorage(new QdrantVectorStorage(new QdrantConfig()))
        .WithQueueStorage(new RabbitMQueueStorage(new RabbitMQConfig()))
        .WithPipelineStorage(new RedisPipelineStorage(new RedisConfig()))
        .BuildHiveMemory();

    return memory;
}
