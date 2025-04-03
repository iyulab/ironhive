using IronHive.Abstractions;
using IronHive.Abstractions.Memory;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Core;
using IronHive.Core.Storages;
using IronHive.Storages.Qdrant;
using IronHive.Storages.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

var text = "Hello, World!";
Console.WriteLine(text);
var coll = "test";

var hive = Create();
//var worker = new PipelineWorker(hive.Services)
//{
//    MaxConcurrent = 3,
//    DelayMilliseconds = 1_000,
//};

//_ = worker.StartAsync(default);
//var memory = hive.Services.GetRequiredService<IMemoryService>();

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
//handlerOptions: new Dictionary<string, object?>
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

//await Task.Delay(60_000);

//var docs = await memory.SearchAsync(coll, "openai", "text-embedding-3-large", "Hello, World!");

//await memory.DeleteCollectionAsync(coll);

var queue = hive.Services.GetRequiredService<IQueueStorage>();

await queue.EnqueueAsync("test");
await queue.EnqueueAsync("test2");
await queue.EnqueueAsync("test3");
await queue.EnqueueAsync("test4");
await queue.EnqueueAsync("test5");

var count = await queue.CountAsync();
Console.WriteLine($"Count: {count}");

for (var i = 0; i < count; i++)
{
    var item = await queue.DequeueAsync<string>();
    if (i == 1)
    {
        await queue.NackAsync(item.AckTag, requeue: true);
        Console.WriteLine($"Nack: {item.Message}");
    }
    else if (i == 2)
    {
        await queue.NackAsync(item.AckTag, requeue: false);
        Console.WriteLine($"Nack: {item.Message}");
    }
    else
    {
        await queue.AckAsync(item.AckTag);
        Console.WriteLine($"Ack: {item.Message}");
    }
    await Task.Delay(5_000);
}

await queue.ClearAsync();

return;

IHiveMind Create()
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

    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSingleton<IPipelineEventHandler, LogConsole>();

    var mind = new HiveServiceBuilder(services)
        .AddDefaultFileStorages()
        .AddDefaultFileDecoders()
        .AddDefaultPipelineHandlers()
        .AddChatCompletionConnector("openai", new OpenAIChatCompletionConnector(o_config))
        .AddEmbeddingConnector("openai", new OpenAIEmbeddingConnector(o_config))
        .AddChatCompletionConnector("anthropic", new AnthropicChatCompletionConnector(a_config))
        .AddChatCompletionConnector("gemini", new OpenAIChatCompletionConnector(g_config))
        .AddChatCompletionConnector("iyulab", new OpenAIChatCompletionConnector(l_config))
        .WithQueueStorage(new RabbitMQueueStorage(new RabbitMQConfig()))
        .WithVectorStorage(new QdrantVectorStorage(new QdrantConfig()))
        .BuildHiveMind();

    return mind;
}

public class LogConsole : IPipelineEventHandler
{
    public Task OnCompletedAsync(string pipelineId)
    {
        Console.WriteLine($"Completed: {pipelineId}");
        return Task.CompletedTask;
    }

    public Task OnFailedAsync(string pipelineId, Exception exception)
    {
        Console.WriteLine($"Failed: {pipelineId} - {exception.Message}");
        return Task.CompletedTask;
    }

    public Task OnProcessAfterAsync(string pipelineId, string step, PipelineContext context)
    {
        Console.WriteLine($"After: {pipelineId} - {step}");
        return Task.CompletedTask;
    }

    public Task OnProcessBeforeAsync(string pipelineId, string step, PipelineContext context)
    {
        Console.WriteLine($"Before: {pipelineId} - {step}");
        return Task.CompletedTask;
    }

    public Task OnQueuedAsync(string pipelineId)
    {
        Console.WriteLine($"Queued: {pipelineId}");
        return Task.CompletedTask;
    }
}
