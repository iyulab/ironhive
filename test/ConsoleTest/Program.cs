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
using IronHive.Core.Memory.Handlers;
using IronHive.Core.Storages;
using IronHive.Storages.Qdrant;
using IronHive.Storages.RabbitMQ;
using IronHive.Storages.Redis;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var text = "Hello, World!";
Console.WriteLine(text);
var coll = "test_1";

//var queue = new RabbitMQueueStorage(new RabbitMQConfig());
var queue = new LocalQueueStorage();
var memory = Create();


var msg = await queue.GetCountAsync();

var list = new List<string>()
{
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
    Guid.NewGuid().ToString(),
};

foreach (var item in list)
{
    await queue.EnqueueAsync(item);
    await Task.Delay(5_000);
    Console.WriteLine($"Enqueue: {item}");
}

var total = await queue.GetCountAsync();
Console.WriteLine($"Total: {total}");

for (int i = 0; i < total; i++)
{
    var item = await queue.DequeueAsync<string>();
    await Task.Delay(5_000);
    Console.WriteLine($"Dequeue: {item}");
}

//await memory.CreateCollectionAsync(coll, "openai", "text-embedding-3-large");

//await memory.MemorizeAsync(coll, new FileMemorySource
//{
//    Provider = "local",
//    Id = Guid.NewGuid().ToString(),
//    FilePath = "c:\\temp\\test.txt",
//},
//[
//    "decode",
//    "chunk",
//    "qnagen",
//    "embed",
//]);

//await memory.DeleteCollectionAsync(coll);

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
        .WithVectorStorage(new QdrantVectorStorage(new QdrantConfig()))
        .AddFileStorage("local", (sp, config) => new LocalFileStorage())
        .WithQueueStorage(new RabbitMQueueStorage(new RabbitMQConfig()))
        .WithPipelineStorage(new RedisPipelineStorage(new RedisConfig()))
        .BuildHiveMemory();

    return memory;
}
