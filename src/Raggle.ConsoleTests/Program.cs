
using LLama;
using LLama.Common;
using LLama.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Raggle.Engines.Anthropic;
using Raggle.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

//var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
//var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
//var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;

//var result = NativeApi.llama_max_devices();
//Console.WriteLine($"Max Devices: {result}");
//return;

var timer = new Stopwatch();

var systemPrompt = "Just Answer json format";
var userPrompt = "What are you";
//NativeLibraryConfig.All.WithLogCallback((level, message) => { });

await Infer();
return;

var configuration = new ConfigurationBuilder()
    .AddJsonFile(@"C:\data\Raggle\src\Raggle.ConsoleTests\User.json", false, true)
    .Build();

var services = new ServiceCollection();
services.Configure<ServiceOptions>(configuration.GetSection("Raggle"));
services.AddSingleton<RaggleService>();
var provider = services.BuildServiceProvider();

//await Ask("openai/gpt-4o-2024-08-06");

foreach (var i in Enumerable.Range(0, 100))
{
    Console.WriteLine($"This is {i + 1} count asking");

    //await Ask("Mistral-7B-Instruct-v0.3.Q4_K_M");

    //await Ask("Grabbe-AI.Q4_K_M");

    await Ask("Meta-Llama-3.1-8B-Instruct-Q4_K_M");

    //await Ask("codellama-7b-instruct.Q4_K_M");

    //await Ask("qwen2-0_5b-instruct-q4_k_m");
}

async Task Ask(string modelId)
{
    var raggle = provider.GetRequiredService<RaggleService>();
    var history = new ChatHistory();
    
    history.AddSystemMessage(systemPrompt);
    history.AddUserMessage(userPrompt);
    Console.WriteLine($"Asking to {modelId}");
    timer.Reset();
    timer.Start();
    await foreach (var message in raggle.AskAsync(modelId, history))
    {
        Console.Write(message);
    }
    timer.Stop();
    Console.WriteLine();
    Console.WriteLine($"Elapsed Time: {timer.Elapsed}");
    Console.WriteLine();
}

async Task Infer()
{
    var modelPath = @"C:\Models\test_Q4_K_M.gguf";
    var parameters = new ModelParams(modelPath)
    {
        ContextSize = 1024,
        Seed = 20022,
    };
    var model = LLamaWeights.LoadFromFile(parameters);
    var executor = new StatelessExecutor(model, parameters);

    var prompt = $"""
        <s>
        <|system|>{systemPrompt}<|end|>
        <|user|>{userPrompt}<|end|>
        <|assistant|>
        """;

    //var prompt = "<|begin_of_text|><|start_header_id|>system<|end_header_id|>" +
    //         systemPrompt +
    //         "<|eot_id|><|start_header_id|>user<|end_header_id|>" +
    //         userPrompt +
    //         "<|eot_id|><|start_header_id|>assistant<|end_header_id|>";

    var tokens = model.Tokenize(prompt, false, true, Encoding.UTF8);
    //var context = model.CreateContext(parameters);
    
    Console.WriteLine($"Token Counted: {tokens.Length}");
    await foreach(var result in executor.InferAsync(prompt))
    {
        Console.Write(result);
    }
}
